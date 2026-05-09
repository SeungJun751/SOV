using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI; 

public class FirebaseDataReceiver : MonoBehaviour
{
    private DatabaseReference reference;
    private FirebaseAuth auth;
    public string dataPath = "sensor/cycle_dr_rotation";
    public static int CurrentCount = 0;
    public UnityEvent<int> OnCountUpdated;

    void Start()
    {
        Debug.Log("Firebase 초기화 시도...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                auth.SignInAnonymouslyAsync().ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled || authTask.IsFaulted) { Debug.LogError("익명 로그인 실패"); return; }

                    Debug.Log("Firebase 익명 로그인 성공!");
                    reference = FirebaseDatabase.DefaultInstance.GetReference(dataPath);

                    reference.SetValueAsync(0);

                    reference.ValueChanged += HandleValueChanged;
                    Debug.Log("Firebase 데이터 리스너 등록 완료.");
                });
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패");
            }
        });
    }

    private void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) { Debug.LogError(args.DatabaseError.Message); return; }

        if (args.Snapshot != null && args.Snapshot.Value != null)
        {
            int newCount = int.Parse(args.Snapshot.Value.ToString());
            CurrentCount = newCount;
            OnCountUpdated.Invoke(newCount);

            Debug.Log($"새로운 카운트 값 수신: {newCount}");
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