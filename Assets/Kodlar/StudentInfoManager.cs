using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StudentInfoManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;
    bool isFirebaseReady = false; // ← إضافة هذا

    public TMP_InputField nameInputField;
    public TMP_InputField studentNumberInputField;
    public TMP_Text warningText;
    public GameObject studentInfoPanel;
    public GameObject quizPanel;
    public QuizManager quizManager;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) // ← تأكد Firebase شغال
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseReady = true; // ← Firebase جاهز
                Debug.Log("Firebase is ready.");
            }
            else
            {
                Debug.LogError("Firebase غير متاح: " + task.Result);
            }
        });
    }

    void SaveStudentData(string studentName, string studentNumber)
    {
        if (!isFirebaseReady || db == null) // ← تحقق قبل الاستخدام
        {
            Debug.LogError("Firebase غير جاهز بعد!");
            return;
        }

        DocumentReference docRef = db.Collection("students").Document(studentNumber);
        Dictionary<string, object> studentData = new Dictionary<string, object>
        {
            { "name", studentName },
            { "student_number", studentNumber },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        docRef.SetAsync(studentData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("تم حفظ البيانات بنجاح.");
            }
            else
            {
                Debug.LogError("حدث خطأ أثناء حفظ البيانات: " + task.Exception);
            }
        });
    }

    public void StartExam()
    {
        string studentName = nameInputField.text.Trim();
        string studentNumber = studentNumberInputField.text.Trim();

        if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentNumber))
        {
            if (warningText != null)
                warningText.text = "يرجى ملء جميع الحقول.";
            return;
        }

        if (!isFirebaseReady) // ← تحقق قبل البدء
        {
            if (warningText != null)
                warningText.text = "جاري تهيئة النظام، حاول مرة أخرى.";
            return;
        }

        if (warningText != null)
            warningText.text = "";

        if (studentInfoPanel != null)
            studentInfoPanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (quizManager != null)
            quizManager.StartQuiz(studentName, studentNumber);

        SaveStudentData(studentName, studentNumber);
    }
}