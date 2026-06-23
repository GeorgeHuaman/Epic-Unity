using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 10);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 45.0f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 100;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "Idle": break;
                case "WaitingForCompile":
                    EditorApplication.delayCall += () => {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying) {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying) EditorApplication.update += WaitFramesThenRun;
                    break;
                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode) {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;
        
        // Custom state
        private static bool _generationRequested = false;
        private static bool _generationFinished = false;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone) {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try { Setup(); }
                catch (System.Exception e) { FinishTest(true, "Setup error: " + e.Message); }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try {
                if (Tick(elapsed) || timedOut) {
                    FinishTest(timedOut && !_generationFinished, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e) { FinishTest(true, "Tick error: " + e.Message); }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            if (_testDone) return;
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;
            string resultJson = GetResult();
            if (isError && errorMessage != null) {
                resultJson = JsonUtility.ToJson(new TestResult { success = false, error = errorMessage, logs = _capturedLogs.ToArray() });
            }
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            _capturedLogs.Add("[" + type + "] " + message);
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult {
            public bool success;
            public string error;
            public int npcCount;
            public string[] logs;
        }

        // ============================================================
        // TEST LOGIC
        // ============================================================

        private static void Setup()
        {
            Debug.Log("[Test] Buscando componentes para iniciar la generación...");
            UIManager ui = Object.FindAnyObjectByType<UIManager>();
            WorldBuilder builder = Object.FindAnyObjectByType<WorldBuilder>();
            
            if (ui == null || builder == null || ui.aiConnector == null || builder.proceduralGenerator == null) {
                throw new System.Exception("No se encontraron UIManager, WorldBuilder o dependencias necesarias.");
            }

            builder.proceduralGenerator.OnGenerationComplete += () => {
                Debug.Log("[Test] OnGenerationComplete disparado por el generador.");
                // Wait a bit more in Tick before finishing
                _generationFinished = true;
            };

            string prompt = "Crea un laboratorio de 4 salas, tambien que halla un npc en la primera sala que al acercarme me diga 'Hola aventurero, ayudame a resolver el problema quimico', en la 2 sala debe haber un npc la mision de pregunta que diga Se disuelven 20 gramos de sal (NaCl) en agua hasta obtener una solución con un volumen final de 500 mL. ¿Cuál es la concentración de la solución en g/L?, las opciones son A) 10 g/L,B) 20 g/L,C) 40 g/L,D) 80 g/L, la respuesta correcta es la C, y en la 3 sala debe haber un npc con la mision de pregunta que diga ¿Cuántos moles de agua (H2O) se producen?, y las respuesta sea, A) 1 mol de H2O, B) 2 moles de H2O, C) 3 moles de H2O, D) 4 moles de H2O y la respuesta correcta sea la B";

            Debug.Log("[Test] Enviando prompt a la IA...");
            ui.StartCoroutine(ui.aiConnector.EnviarPromptALaIA(prompt, (config) => {
                Debug.Log("[Test] Respuesta de IA recibida. Iniciando construcción del mundo.");
                builder.ConstruirMundo(config);
            }));
            
            _generationRequested = true;
        }

        private static float _finishTime = 0;
        private static bool Tick(float elapsed)
        {
            // Wait for generation to finish and then a bit more for NPCs to settle/spawn
            if (_generationFinished) {
                if (_finishTime == 0) _finishTime = elapsed;
                return (elapsed - _finishTime) > 2.0f; // Wait 2 extra seconds after complete
            }
            return false;
        }

        private static string GetResult()
        {
            GameObject[] allNPCs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int count = 0;
            foreach(var go in allNPCs) {
                if (go.name == "NPCs" || go.name.Contains("NPCs")) count++;
            }

            Debug.Log("[Test] Resultado final: " + count + " NPCs encontrados.");
            
            var result = new TestResult {
                success = (count == 3),
                npcCount = count,
                logs = _capturedLogs.ToArray()
            };
            if (count != 3) result.error = "Se esperaban 3 NPCs, pero se encontraron " + count;
            
            return JsonUtility.ToJson(result);
        }
    }
}
