using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Engine;
using Fusee.Engine.SimpleScene;
using Fusee.SceneManagement;
using Fusee.Math;
using Fusee.Serialization;
using System.Windows.Forms;

namespace Examples.BeeTheGame
{
    class GUIRender : RenderCanvas
    {
        private int _guiScore;

        private GUIHandler _guiHandler;
        private RenderContext RC;
        private IFont _guiFontArial24;
        private GUIText _guiArial24;
        private GUIText _guiTextScore;
        private GUIImage _guiImage;
        private GUIImage _guiImagePause;
        private GUIImage _guiImageDummy;
        private GUIImage _guiImageBar;
        private GUIImage _guiImageBarText;
        private GUIImage _guiImageStartBack;
        private GUIImage _guiImageStartBee;
        private GUIImage _guiImageStartPlay;
        private GUIImage _guiImageGameHelp;
        private GUIImage _guiImageHelpOne;
        private GUIImage _guiImageHelpTwo;
        private GUIImage _guiImageHelpButton;

        private GUIButton _guiButtonPlay;
        private GUIButton _guiButtonHelp;

        private GUIImage _guiImageContainer;
        private GUIImage[] _guiImageContainerArray = new GUIImage[5];

        private BeeTheGame _game;

        // Konstruktor.
        public GUIRender(RenderContext rc, BeeTheGame thisGame)
        {
            RC = rc;
            _game = thisGame;
            //Attach the GUIHandler to this Context
            _guiHandler = new GUIHandler();
            _guiHandler.AttachToContext(rc);

            //Image
            _guiImageDummy = new GUIImage("Assets/dummy.png", 0, 0, 0, 5, 5);
            _guiImageBarText = new GUIImage("Assets/nectarCollected.png", 10, 10, -1, 153, 23);

            _guiImageStartBack = new GUIImage("Assets/background.png", 0, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/9)*2);
            _guiImageGameHelp = new GUIImage("Assets/gameHelp.png", 0, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/9)*2);
            _guiImageStartBee = new GUIImage("Assets/beePlaceholder.png", 200, 30, 1, 130, 118);
            _guiImageStartPlay = new GUIImage("Assets/play.png", (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width/2 - 300/2), 0, 2, 300, (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/9)*2);

            _guiImageHelpButton = new GUIImage("Assets/helpButton.png", 10, 10, 2, 41, 50);
            _guiImageHelpOne = new GUIImage("Assets/helpOne.png", 100, 10, 1, 300, 129);
            _guiImageHelpTwo = new GUIImage("Assets/helpTwo.png", 950, 20, 1, 300, 121);

            //Text
            _guiFontArial24 = RC.LoadFont("Assets/arial.ttf", 16);
            _guiArial24 = new GUIText("Score: ", _guiFontArial24, 1200, 25);
            _guiArial24.TextColor = new float4(1, 1, 1, 1);

            //Button
            _guiButtonPlay = new GUIButton((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width/2 - 300/2), 0, 3, 300, (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height/9)*2);
            
            _guiButtonPlay.ButtonColor = new float4(0, 0, 0, 0);
            _guiButtonPlay.BorderColor = new float4(0, 0, 0, 1);
            _guiButtonPlay.BorderWidth = 0;

            _guiButtonHelp = new GUIButton(10, 10, 1, 50, 50);

            _guiButtonHelp.ButtonColor = new float4(0, 0, 0, 0);
            _guiButtonHelp.BorderColor = new float4(0, 0, 0, 1);
            _guiButtonHelp.BorderWidth = 0;

            _guiHandler.Add(_guiImageDummy);
            _guiHandler.Add(_guiImageBarText);

            _guiImagePause = new GUIImage("Assets/gamePaused.png", 350, 40, 0, 500, 100);
        }

        public void SetGUIScore(int _score)
        {
            //get score, display upper right
            _guiScore = _score;

            _guiHandler.Remove(_guiTextScore);
            _guiTextScore = new GUIText(" " + _guiScore, _guiFontArial24, 1245, 25);
            _guiTextScore.TextColor = new float4(1, 1, 1, 1);
            _guiHandler.Add(_guiArial24);
            _guiHandler.Add(_guiTextScore);
        }

        public void StartMenue()
        {
            _guiButtonPlay.OnGUIButtonDown += OnPlayButtonDown;
            _guiButtonHelp.OnGUIButtonDown += OnHelpButtonDown;

            _guiHandler.Remove(_guiTextScore);

            _guiHandler.Add(_guiButtonPlay);
            _guiHandler.Add(_guiButtonHelp);
            _guiHandler.Add(_guiImageHelpButton);
            _guiHandler.Add(_guiImageStartBack);
            _guiHandler.Add(_guiImageStartBee);
            _guiHandler.Add(_guiImageStartPlay);
        }

        private void OnPlayButtonDown(GUIButton sender, Fusee.Engine.MouseEventArgs mea)
        {
            _guiHandler.Remove(_guiButtonPlay);
            _guiHandler.Remove(_guiImageHelpButton);
            _guiHandler.Remove(_guiButtonHelp);
            _guiHandler.Remove(_guiImageStartBack);
            _guiHandler.Remove(_guiImageStartBee);
            _guiHandler.Remove(_guiImageStartPlay);
            _guiHandler.Remove(_guiImageGameHelp);
            _guiHandler.Remove(_guiImageHelpOne);
            _guiHandler.Remove(_guiImageHelpTwo);

            _game._gameState = GameState.InGame;
        }

        private void OnHelpButtonDown(GUIButton sender, Fusee.Engine.MouseEventArgs mea)
        {
            _guiButtonPlay.OnGUIButtonDown += OnPlayButtonDown;
            _guiButtonPlay.OnGUIButtonDown += OnPlayButtonDown;

            _guiHandler.Remove(_guiImageStartBack);
            _guiHandler.Remove(_guiImageStartBee);
            _guiHandler.Remove(_guiButtonHelp);
            _guiHandler.Remove(_guiTextScore);
            _guiHandler.Remove(_guiImageHelpButton);

            _guiHandler.Add(_guiImageGameHelp);
            _guiHandler.Add(_guiImageHelpOne);
            _guiHandler.Add(_guiImageHelpTwo);

            _game._gameState = GameState.GameHelp;
        }


        public void RenderPause()
        {            
            _guiHandler.Add(_guiImagePause);
        }

        public void DeletePause()
        {
            _guiHandler.Remove(_guiImagePause);
        }

        public void RenderIngame()
        {
            _guiHandler.RenderGUI();
        }

        public void addNectar(int _punktePos)
        {
            //nectar blocks have to be added
            _guiImageContainerArray[_punktePos-1] = new GUIImage("Assets/honeycombContainer.png", 155+(_punktePos*20), 10, 0, 20, 20);
            _guiHandler.Add(_guiImageContainerArray[_punktePos-1]);
        }

        public void removeNectar(int _punktePos)
        {
            //nectar blocks have to be removed
            _guiHandler.Remove(_guiImageContainerArray[_punktePos]);
        }

        public void Refresh()
        {
            _guiHandler.Refresh();
        }


    }
}