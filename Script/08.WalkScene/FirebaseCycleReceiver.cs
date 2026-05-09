using UnityEngine;
using UnityEngine.UI; 
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;

public class FirebaseCycleReceiver : MonoBehaviour
{
    [Header("Firebase Settings")]
    public string dataPath = "sensor/cycle_dr_rotation";

    [Header("UI Display")]
    public Text debugText;
    public static int RotationCount = 0;
    private Quaternion lastRotation = Quaternion.identity;
    private float accumulatedAngle = 0f;
    private bool isFirstDataReceived = false;

    private DatabaseReference reference;
    private FirebaseAuth auth;

    void Start()
    {
        Debug.Log("Firebase 초기화 시도...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result != DependencyStatus.Available) { Debug.LogError("Firebase 초기화 실패"); return; }

            auth = FirebaseAuth.DefaultInstance;
            auth.SignInAnonymouslyAsync().ContinueWith(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted) { Debug.LogError("익명 로그인 실패"); return; }

                Debug.Log("Firebase 익명 로그인 성공!");
                reference = FirebaseDatabase.DefaultInstance.GetReference(dataPath);

                reference.ValueChanged += HandleValueChanged;
                Debug.Log("Firebase 회전 데이터 리스너 등록 완료.");
            });
        });
    }

    private void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) { Debug.LogError(args.DatabaseError.Message); return; }

        if (args.Snapshot != null && args.Snapshot.Exists)
        {
            var rawData = args.Snapshot.Value as Dictionary<string, object>;
            if (rawData == null) return;

            float x = System.Convert.ToSingle(rawData["x"]);
            float y = System.Convert.ToSingle(rawData["y"]);
            float z = System.Convert.ToSingle(rawData["z"]);
            float w = System.Convert.ToSingle(rawData["w"]);
            Quaternion currentRotation = new Quaternion(x, y, z, w);

            if (!isFirstDataReceived)
            {
                lastRotation = currentRotation;
                isFirstDataReceived = true;
                return;
            }

            float deltaAngle = Quaternion.Angle(lastRotation, currentRotation);
            accumulatedAngle += deltaAngle;

            if (accumulatedAngle >= 360f)
            {
                RotationCount++;
                accumulatedAngle -= 360f;
                Debug.Log($"1바퀴 회전 감지! 총 회전 수: {RotationCount}");
            }

            lastRotation = currentRotation;
            UpdateDebugText();
        }
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = $"총 회전 수: {RotationCount}\n" +
                             $"누적 각도: {Mathf.FloorToInt(accumulatedAngle)} / 360";
        }
    }

    void OnDestroy()
    {
        if (reference != null)
        {
            reference.ValueChanged -= HandleValueChanged;
        }
    }
}