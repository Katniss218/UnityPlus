using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Console
{
    /// <summary>
    /// Specifies that a given UI element will be used as log output (see <see cref="_output"/>). <br />
    /// Should probably be placed in a scene that doesn't get unloaded.
    /// </summary>
    [DisallowMultipleComponent]
    public class LogTextOutput : MonoBehaviour
    {
        static string LOG_COLOR_INFO = "#dddddd";
        static string LOG_COLOR_WARN = "#dddd55";
        static string LOG_COLOR_ERROR = "#dd5555";

        static string LOG_COLOR_EXCEPTION = "#dd5555";
        static string LOG_COLOR_EXCEPTION_STACK = "#c55555";

        [SerializeField]
        private GameObject _consoleUIElement = null;

        [SerializeField]
        private TMPro.TextMeshProUGUI _output = null;

        [SerializeField]
        private string _defaultText = "Console:\n\n";

        /// <summary>
        /// Prints a string out to the console.
        /// </summary>
        public void Print( string message )
        {
            _output.text += message;
        }

        /// <summary>
        /// Prints a string terminated with a newline character.
        /// </summary>
        public void PrintLine( string message )
        {
            _output.text += $"{message}\n";
        }

        private void HandleLog( string message, string stackTrace, LogType logType )
        {
            switch( logType )
            {
                case LogType.Log:
                    PrintLine( $"<color={LOG_COLOR_INFO}>[{DateTime.Now.ToLongTimeString()}](___) - {message}</color>" );
                    break;

                case LogType.Warning:
                    PrintLine( $"<color={LOG_COLOR_WARN}>[{DateTime.Now.ToLongTimeString()}](WRN) - {message}</color>" );
                    break;

                case LogType.Error:
                    PrintLine( $"<color={LOG_COLOR_ERROR}>[{DateTime.Now.ToLongTimeString()}](ERR) - {message}</color>" );
                    _consoleUIElement.SetActive( true );
                    break;

                case LogType.Exception:
                    PrintLine( $"<color={LOG_COLOR_EXCEPTION}>[{DateTime.Now.ToLongTimeString()}](EXC) - {message}</color>\n  at\n<color={LOG_COLOR_EXCEPTION_STACK}>" + stackTrace + "</color>" );
                    _consoleUIElement.SetActive( true );
                    break;
            }
        }

        void Awake()
        {
            if( _consoleUIElement == null || _consoleUIElement == this.transform )
            {
                throw new InvalidOperationException( $"Can't initialize {nameof( LogTextOutput )} - {nameof( _consoleUIElement )} must be non-null and not equal to the gameobject that this component is attached to." );
            }

            if( !_output.richText )
            {
                Debug.LogWarning( $"The console ({_output}) isn't set to Rich Rext, setting to Rich Text now." );
                _output.richText = true;
            }

            _output.text = _defaultText; // This is required to fix glitch requiring reenabling the gameObject after adding some text to the output (if it's set to blank).
            _consoleUIElement.SetActive( false );
        }

        void OnEnable()
        {
            Application.logMessageReceived += this.HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= this.HandleLog;
        }
    }
}