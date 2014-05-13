using System.Collections.Generic;
using System.IO;
using Fusee.Engine;
using Fusee.Engine.SimpleScene;
using Fusee.SceneManagement;
using Fusee.Math;
using Fusee.Serialization;

namespace Examples.BeeTheGame
{
    public class BeeTheGame : RenderCanvas
    {
        public static SceneObjectContainer FindByName(string name, IEnumerable<SceneObjectContainer> list)
        {
            foreach (SceneObjectContainer soc in list)
            {
                if (name == soc.Name)
                    return soc;

                SceneObjectContainer found = FindByName(name, soc.Children);
                if (found != null)
                    return found;
            }
            return null;
        }

        //private ShaderProgram _myShader;
        private SceneRenderer _sceneRenderer;
        private SceneRenderer _sceneRenderer2;
        private SceneRenderer _sceneRenderer3;
        private SceneObjectContainer _sceneObject;
        private SceneObjectContainer _sceneObject2;
        private SceneObjectContainer _sceneObject3;
        private float _yAngle;
        private float _xPos;
        private float _yPos;
        private bool _rotChanged = false;
        // is called on startup
        public override void Init()
        {
            // _myMesh = new Cube();
            SceneContainer scene;
            SceneContainer scene2;
            SceneContainer scene3;
            var ser = new Serializer();

            using (var file = File.OpenRead(@"Assets/blume_lila_container.fus"))
            {
                scene = ser.Deserialize(file, null, typeof(SceneContainer)) as SceneContainer;
            }

            var ser2 = new Serializer();
            using (var file = File.OpenRead(@"Assets/blume_lila_container.fus"))
            {
                scene2 = ser2.Deserialize(file, null, typeof(SceneContainer)) as SceneContainer;
            }

            var ser3 = new Serializer();
            using (var file = File.OpenRead(@"Assets/blume_lila_container.fus"))
            {
                scene3 = ser3.Deserialize(file, null, typeof(SceneContainer)) as SceneContainer;
            }

            _sceneRenderer = new SceneRenderer(scene, "Assets");
            _sceneRenderer2 = new SceneRenderer(scene2, "Assets");

            _sceneRenderer3 = new SceneRenderer(scene, "Assets");

            _sceneObject = FindByName("gelbemitte", scene.Children);
            _sceneObject2 = FindByName("gelbemitte", scene2.Children);
            

            /*foreach (SceneObjectContainer soc in scene3.Children)
            {
                _sceneObject3 = soc;
            }*/
            //_sceneObject3 = scene3 as SceneObjectContainer;
            _sceneObject3 = FindByName("gelbemitte", scene3.Children);
            /*
            _sceneObject3.Transform.Scale = _sceneObject2.Transform.Scale / 8;
            _sceneObject3.Transform.Translation.x = 0;
            _sceneObject3.Transform.Translation.y = 35;
            _sceneObject3.Transform.Translation.z = 35;
            */

            RC.ClearColor = new float4(0.1f, 0.1f, 0.5f, 1);
            _yAngle = 0;
            _xPos = 0;
        }

        // is called once a frame
        public override void RenderAFrame()
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(500, 120, 280, 0, 420, 280, 0, 1, 0);// * float4x4.CreateRotationY(_yAngle) * float4x4.CreateTranslation(_xPos, 0,0)


            if (_sceneObject != null)
            {
                _sceneObject.Transform.Rotation.y = _yAngle;
                //_sceneObject.Transform.Translation.x = _xPos;
            }
            if (_sceneObject2 != null)
            {
                //_sceneObject2.Transform.Rotation.x = _yAngle;
                _sceneObject2.Transform.Translation.z = _xPos;
                _sceneObject2.Transform.Translation.y = _yPos;
            }
            if (Input.Instance.IsKey(KeyCodes.W))
                _yAngle += 1.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.S))
                _yAngle -= 1.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.Up))
                _yPos += 25.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.Down))
                _yPos -= 25.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.Right))
                _xPos += 100.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.Left))
                _xPos -= 100.0f * (float)Time.Instance.DeltaTime;
            if (Input.Instance.IsKey(KeyCodes.W))
            {
                ChangeBeeRot(true, 1f);
            }
            if (Input.Instance.IsKey(KeyCodes.S))
            {
                ChangeBeeRot(true, -1f);
            }
            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Down))
            {
                ChangeBeeRot(false, 1f);
            }
            _sceneRenderer2.Render(RC);
            _sceneRenderer3.Render(RC);
            _sceneRenderer.Render(RC);
            Present();
        }

        private void ChangeBeeRot(bool what, float dir)
        {
            if (what != _rotChanged || dir != 0.0f)
            {
                if (what == true)
                {
                    _sceneObject2.Transform.Rotation.y = 1.2f * dir;
                }
                else
                {
                    _sceneObject2.Transform.Rotation.y = 0.0f;
                }

                _rotChanged = what;
            }
            return;
        }

        // is called when the window was resized
        public override void Resize()
        {

            //RC.Viewport(0, 0, Width, Height);
            //var aspectRatio = Width / (float)Height;
            //RC.Projection = float4x4.CreateOrthographic(800,600,1,100);
            RC.Viewport(0, 0, Width, Height * 4);
            var aspectRatio = 1f;// OLD: Width / Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, aspectRatio, 280, 10000);
        }

        public static void Main()
        {
            var app = new BeeTheGame();
            app.Run();
        }
    }
}
