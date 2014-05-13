using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Engine;
using Fusee.Engine.SimpleScene;
using Fusee.SceneManagement;
using Fusee.Math;
using Fusee.Serialization;

namespace Examples.MasterFuseeTest
{
    class GUIRender
    {
        private GUIHandler _guiHandler;
        private RenderContext RC;
        private IFont _fontArial;
        private GUIText _guiText;
        private GUIImage _guiImage;

        // Konstruktor.
        public GUIRender(RenderContext rc) {
            RC = rc;

            //Attach the GUIHandler to this Context
            _guiHandler = new GUIHandler();
            _guiHandler.AttachToContext(rc);
            
            //Text
            _fontArial = rc.LoadFont("Assets/arial.ttf", 24);
            _guiText = new GUIText("Spot all seven differences!", _fontArial, 200, 20, 10);
            _guiText.TextColor = new float4(1, 1, 1, 1);
            

            //Image
            _guiImage = new GUIImage("Assets/border.png", 200, 100, 2, 200, 30);
            _guiHandler.Add(_guiImage);
            _guiHandler.Add(_guiText);

        }

     

        public void RenderMainMenue()
        {

        }

        public void RenderIngame()
        {

            //rc.Clear(ClearFlags.Color | ClearFlags.Depth);
            // Solltest hier auf den State checken bevor du renderst.Oder? 
            _guiHandler.RenderGUI();
        }


    }
}