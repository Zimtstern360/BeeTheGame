using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Engine;
using Fusee.Engine.SimpleScene;
using Fusee.SceneManagement;
using Fusee.Math;
using Fusee.Serialization;

namespace Examples.BeeTheGame
{
    class GUIRender : RenderCanvas
    {
        //enum GameState { running, paused };
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

        private GUIImage _guiImageContainer;
        private GUIImage[] _guiImageContainerArray = new GUIImage[5];

        // Konstruktor.
        public GUIRender(RenderContext rc)
        {
            RC = rc;
            //Attach the GUIHandler to this Context
            _guiHandler = new GUIHandler();
            _guiHandler.AttachToContext(rc);

            //Image
            _guiImageDummy = new GUIImage("Assets/dummy.png", 0, 0, 0, 5, 5);
            _guiImageBarText = new GUIImage("Assets/nectarCollected.png", 10, 10, -1, 153, 23);

            //Text
            _guiFontArial24 = RC.LoadFont("Assets/arial.ttf", 16);
            _guiArial24 = new GUIText("Score: ", _guiFontArial24, 1200, 25);
            _guiArial24.TextColor = new float4(1, 1, 1, 1);

            _guiHandler.Add(_guiArial24);
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
            _guiHandler.Add(_guiTextScore);
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