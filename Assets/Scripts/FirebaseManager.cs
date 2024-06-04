using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    //Firebase Variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public static FirebaseAuth auth;
    public FirebaseUser User;

    //Register Variables
    [Header("Register")]
    public TMP_InputField EmailRegisterField;
    public TMP_InputField PasswordRegisterField;
    public TMP_InputField ConfirmPasswordRegisterField;
    public TMP_InputField FirstNameRegisterField;
    public TMP_InputField LastNameRegisterField;
    public TMP_Text WarningRegisterText;
    public Canvas CanvasRegister;

    //Login Variables
    [Header("Login")]
    public TMP_InputField EmailLoginField;
    public TMP_InputField PasswordLoginField;
    public TMP_Text WarningLoginText;
    public Canvas CanvasSignIn;

    //Reset Password Variables
    [Header("ResetPassword")]
    public TMP_InputField EmailResetPasswordField;
    public TMP_Text WarningResetPasswordText;
    public Canvas CanvasResetPassword;
    public Canvas CanvasStartMenu;

    private void Start()
    {
		//FirebaseApp.LogLevel = LogLevel.Debug;
		//Check that all of the necessary dependencies for Firebase are present on the system
		//FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
  //      {
		//	dependencyStatus = task.Result;
  //          if (dependencyStatus == DependencyStatus.Available)
  //          {
  //              //If they are available Initialize Firebase
  //              InitializeFirebase();
  //          }
  //          else
  //          {
  //              Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
		//	}
  //      });
    }

    public static void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
    }

    //Function for register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email and password
        Debug.Log("Tried to register?");
        StartCoroutine(Register(EmailRegisterField.text, PasswordRegisterField.text));
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password

        StartCoroutine(Login(EmailLoginField.text, PasswordLoginField.text));
    }

    //Function for the logout button
    public void LogoutButton()
    {
        SceneManager.LoadScene(sceneName: "Login");
    }

    //Function for the reset password button
    public void ResetPasswordButton()
    {
        //Call the reset password coroutine passing the email and password
        StartCoroutine(Timeout(60.0f, "Login Request has timed out."));
        StartCoroutine(ResetPassword(EmailResetPasswordField.text));
    }


    async void Call()
    {
        Debug.Log("Started firestore");
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(User.UserId);
        Dictionary<string, object> user = new Dictionary<string, object>
                {
                    { "first_name", FirstNameRegisterField.text },
                    { "last_name", LastNameRegisterField.text },
                    { "email", EmailRegisterField.text },
                };
        Debug.Log("I am here");
        await docRef.SetAsync(user, SetOptions.MergeAll);
    }

    private void Storage()
    {
        Debug.Log("Started Storage task");
        // Get a reference to the storage service, using the default Firebase App
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;

        // Create a storage reference from our storage service
        StorageReference storageRef =
            storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com");

        // File located on disk
        string localFile = @"F:\Unity Projects\RT-Chant\Assets\Recordings\river.jpg";

        // Create a reference to the file you want to upload
        StorageReference riversRef = storageRef.Child(User.UserId + "/river.jpg");

        // Upload the file to the path "images/rivers.jpg"
        riversRef.PutFileAsync(localFile)
            .ContinueWith((Task<StorageMetadata> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else
                {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("md5 hash = " + md5Hash);
                }
            });
    }

    private IEnumerator Register(string _email, string _password)
    {
        if (PasswordRegisterField.text != ConfirmPasswordRegisterField.text)
        {
            //If the passwords don't match show warning
            WarningRegisterText.text = "Passwords Do Not Match";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Registration Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                WarningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                Debug.Log(User.UserId);

                Call();

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _email };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        WarningRegisterText.text = "Email Set Failed!";
                    }
                    else
                    {
                        //Email is now set
                        //Now return to login screen
                        WarningRegisterText.text = "";
                        CanvasRegister.gameObject.SetActive(false);
                        CanvasSignIn.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

	private void ReloadLoginPage() {
        SceneManager.LoadScene(1);
	}

    private IEnumerator Timeout(float a_time, string a_timeoutMessage) {
        yield return new WaitForSecondsRealtime(a_time);
        StopAllCoroutines();
        Debug.LogError("Console: " + a_timeoutMessage);
        MessageManager.QueueMessage(a_timeoutMessage, ReloadLoginPage);
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        if (LoginTask.Exception != null)
        {
			//If there are no errors handle them
			Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Incorrect Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "InvalidEmail";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            WarningLoginText.text = message;
            StopAllCoroutines();
		}
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.UserId);
            WarningLoginText.text = "";
            Storage();
            StopAllCoroutines();
            SceneManager.LoadScene(2);
        }
	}

    private IEnumerator ResetPassword(string _email)
    {
        //Call the firebase reset password function passing the email
        var ResetPasswordTask = auth.SendPasswordResetEmailAsync(_email);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ResetPasswordTask.IsCompleted);

        if (ResetPasswordTask.Exception != null)
        {
            if (ResetPasswordTask.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled.");
            }
            if (ResetPasswordTask.IsFaulted)
            {
                Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + ResetPasswordTask.Exception);
            }
        }
        else
        {
            Debug.Log("Password reset email sent successfully.");
            WarningResetPasswordText.text = "";
            CanvasResetPassword.gameObject.SetActive(false);
            CanvasStartMenu.gameObject.SetActive(true);
        }
    }
}