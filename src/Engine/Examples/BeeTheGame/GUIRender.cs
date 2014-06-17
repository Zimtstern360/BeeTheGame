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
            _guiImage = new GUIImage("Assets/border.png", 0, 30, 0, 300, 30);
            _guiImagePause = new GUIImage("Assets/pause.png", 0, 80, -1, 300, 30);
            _guiHandler.Add(_guiImage);
            _guiHandler.Add(_guiImagePause);


        }



        public void RenderMainMenue()
        {

        }

        public void RenderIngame()
        {
            
            //RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            _guiHandler.RenderGUI();
        }

        /*public void Pause(Boolean _running)
        {
            if (_running == false)
            {
                _guiImagePause = new GUIImage("Assets/pause.png", 300, 80, 300, 30);
                _guiHandler.Remove(_guiImage);
                _guiHandler.Add(_guiImagePause);
            }
            if (_running == true)
            {
                RenderIngame();
            }

        }*/

        public void Refresh()
        {
            _guiImage.Refresh();
            _guiHandler.Refresh();
        }


    }
}