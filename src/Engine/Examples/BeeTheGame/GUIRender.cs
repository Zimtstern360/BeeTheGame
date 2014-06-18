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

        private GUIHandler _guiHandler;
        private RenderContext RC;
        private IFont _fontArial;
        private GUIText _guiText;
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

            //Text
            /*_fontArial = rc.LoadFont("Assets/arial.ttf", 24);
            _guiText = new GUIText("Spot all seven differences!", _fontArial, 200, 200, 10);
            _guiText.TextColor = new float4(1, 1, 1, 1);*/


            //Image
            _guiImageDummy = new GUIImage("Assets/dummy.png", 0, 0, 0, 5, 5);
            _guiImageBar = new GUIImage("Assets/nectarBar.png", 10, 10, -2, 250, 15);
            _guiImageBarText = new GUIImage("Assets/nectarText.png", 10, 10, -1, 150, 15);
            _guiHandler.Add(_guiImageDummy);
            _guiHandler.Add(_guiImageBar);
            _guiHandler.Add(_guiImageBarText);

            _guiImagePause = new GUIImage("Assets/gamePaused.png", 350, 40, 0, 500, 100);
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
            _guiImageContainerArray[_punktePos-1] = new GUIImage("Assets/container.png", 120+(_punktePos*20), 10, 0, 10, 15);
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