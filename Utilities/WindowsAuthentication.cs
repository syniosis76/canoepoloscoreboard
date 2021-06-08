using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Windows;
using System.DirectoryServices.AccountManagement;

namespace Utilities
{
    public class WindowsAuthentication
    {
        public struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [DllImport("credui")]
        private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO creditUR,
              string targetName,
              IntPtr reserved1,
              int iError,
              StringBuilder userName,
              int maxUserName,
              StringBuilder password,
              int maxPassword,
              [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
              CREDUI_FLAGS flags);

        [Flags]
        enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 0x1,
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000,
        }

        public enum CredUIReturnCodes
        {
            NO_ERROR = 0,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004,
        }

        /// <summary>
        /// Prompts for password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if no errors.</returns>
        public bool PromptForPassword(string message, bool showError)
        {
            // Setup the flags and variables
            StringBuilder userPassword = new StringBuilder(), userID = new StringBuilder();
            CREDUI_INFO credUI = new CREDUI_INFO
            {
                pszCaptionText = "Authenticate",
                pszMessageText = message
            };

            credUI.cbSize = Marshal.SizeOf(credUI);
            bool save = true;                        
            CREDUI_FLAGS flags = CREDUI_FLAGS.ALWAYS_SHOW_UI | CREDUI_FLAGS.GENERIC_CREDENTIALS | CREDUI_FLAGS.DO_NOT_PERSIST | CREDUI_FLAGS.EXCLUDE_CERTIFICATES | CREDUI_FLAGS.VALIDATE_USERNAME | CREDUI_FLAGS.REQUEST_ADMINISTRATOR;

            // Prompt the user
            CredUIReturnCodes returnCode = CredUIPromptForCredentials(ref credUI, Environment.MachineName, IntPtr.Zero, 0, userID, 100, userPassword, 100, ref save, flags);
            if (returnCode == CredUIReturnCodes.NO_ERROR)
            {                
                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    if (context.ValidateCredentials(userID.ToString(), userPassword.ToString()))
                    {
                        return true;
                    }
                }
            }

            // Invalid credentials or cancelled.
            if (showError && returnCode != CredUIReturnCodes.ERROR_CANCELLED)
            {
                MessageBox.Show("You have entered invalid credentials.");                
            }

            return false;
        }

        public static bool Authenticate(string message, bool showError)
        {
            WindowsAuthentication authentication = new WindowsAuthentication();
            return authentication.PromptForPassword(message, showError);            
        }
    }
}
