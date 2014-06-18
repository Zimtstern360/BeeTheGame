using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Fusee.Engine;
using Fusee.Engine.SimpleScene;
using Fusee.SceneManagement;
using Fusee.Math;
using Fusee.Serialization;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Examples.BeeTheGame
{
    public enum GameState { Paused, InGame, RotatingW, RotatingS, GameOver, GameStart };

    public class BeeTheGame : RenderCanvas
    {
        private float _yAngle;
        private float _xPos;
        private float _yPos;
        private bool _rotChanged = false;
        private bool _groesse = false;
        private bool _voll = false;
        private int _punkte = 0;

        private String[] assetsStrings = { "blume_blau", "blume_gold", "blume_lila" };
        private String[] assetsContStrings = { "blume_blau_container", "blume_gold_container", "blume_lila_container" };

        private float _newRot;

        private int _arrayLength = 10;
        private int _lanesArray = 6;

        private int _currentLane = 0;

        //Automatische Erkennung der Bildschirmsuflösung
        private int _screenWidth = 800;
        private int _screenHeight = 600;
        private int _screenWidthAspect = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        private int _screenHeightAspect = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        private float _twoPi = (float)Math.Round(MathHelper.TwoPi, 6);

        private GameState _gameState;

        private SceneObjectContainer _levelSOC;
        private SceneRenderer _levelSR;
        private SceneContainer _levelSC;

        private SceneObjectContainer _stockSOC;
        private SceneRenderer _stockSR;
        private SceneContainer _stockSC;

        private SceneObjectContainer _playerSOC;
        private SceneRenderer _playerSR;
        private SceneContainer _playerSC;

        private SceneObjectContainer[][] _sOClist;
        private SceneRenderer[][] _sRlist;
        private SceneContainer[][] _scene;
        // private int allObjCount = 0;

        //GUI Stuff
        private GUIRender _guiRender;

        public override void Init()
        {
             

            // DENIZ

            _screenWidth = Screen.PrimaryScreen.Bounds.Width;
            _screenWidthAspect = 1;
            _screenHeight = Screen.PrimaryScreen.Bounds.Height;
            _screenHeightAspect = 1;
            SetWindowSize(_screenWidth, _screenHeight / 9 * 2, true, 0, 0);

            var seri = new Serializer();

            #region LevelInit
            using (var fileLevel = File.OpenRead(@"Assets/ground.fus"))
            {
                _levelSC = seri.Deserialize(fileLevel, null, typeof(SceneContainer)) as SceneContainer;
            }
            _levelSR = new SceneRenderer(_levelSC, "Assets");
            _levelSOC = FindByName("landscape_container", _levelSC.Children);
            //_levelSOC.Transform.Scale = _levelSOC.Transform.Scale / 30;

            using (var fileLevel = File.OpenRead(@"Assets/Bienenstock.fus"))
            {
                _stockSC = seri.Deserialize(fileLevel, null, typeof(SceneContainer)) as SceneContainer;
            }
            _stockSR = new SceneRenderer(_stockSC, "Assets");
            _stockSOC = FindByName("bienenstock", _stockSC.Children);
            #endregion

            #region PlayerInit
            using (var filePlayer = File.OpenRead(@"Assets/blume_lila.fus"))
            {
                _playerSC = seri.Deserialize(filePlayer, null, typeof(SceneContainer)) as SceneContainer;
            }
            _playerSR = new SceneRenderer(_playerSC, "Assets");
            _playerSOC = FindByName("blume_lila_container", _playerSC.Children);
            _playerSOC.Transform.Scale = _playerSOC.Transform.Scale / 5;

            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = 200;
                _playerSOC.Transform.Translation.y = 500;
            }
            #endregion

            #region ArrayInit
            _sOClist = new SceneObjectContainer[_lanesArray][];
            _sRlist = new SceneRenderer[_lanesArray][];
            _scene = new SceneContainer[_lanesArray][];

            for (int c1 = 0; c1 < _sOClist.Length; c1++)
            {
                _sOClist[c1] = new SceneObjectContainer[_arrayLength];
            }
            for (int c2 = 0; c2 < _sOClist.Length; c2++)
            {
                _sRlist[c2] = new SceneRenderer[_arrayLength];
            }
            for (int c3 = 0; c3 < _sOClist.Length; c3++)
            {
                _scene[c3] = new SceneContainer[_arrayLength];
            }
            #endregion

            for (int aCount = 0; aCount < 15; aCount++)
            {
                SpawnFlower();
            }

            RC.ClearColor = new float4(0.1f, 0.1f, 0.5f, 1);
            _yPos = 100;
            _xPos = 150;
            _gameState = GameState.InGame;

            //GUI Stuff
            _guiRender = new GUIRender(RC);
            
        }

        private void SpawnFlower()
        {
            Random rnd = new Random();
            int randomLane = rnd.Next(_lanesArray);
            int randomGrid = rnd.Next(_arrayLength);
            //Lane und Pos check
            while (_sOClist[randomLane][randomGrid] != null)
            {
                randomLane = rnd.Next(_lanesArray - 1);
                randomGrid = rnd.Next(_arrayLength - 1);
            }
            //RND Asset
            int assetsInt = rnd.Next(assetsStrings.Length);
            loadC4D(assetsStrings[assetsInt], randomLane, randomGrid, assetsContStrings[assetsInt]);
            _sOClist[randomLane][randomGrid].Transform.Scale = _sOClist[randomLane][randomGrid].Transform.Scale / 3;
            _sOClist[randomLane][randomGrid].Transform.Translation.x = -25;
            _sOClist[randomLane][randomGrid].Transform.Translation.y = 45; //Höhe?//ok?
            _sOClist[randomLane][randomGrid].Transform.Translation.z = 1400 * (float)(randomGrid + 1) / _arrayLength;
            _sOClist[randomLane][randomGrid].Transform.Rotation.y = 90;
        }

        private void loadC4D(string name, int lane, int place, string childName)
        {
            var ser = new Serializer();

            using (var file = File.OpenRead(@"Assets/" + name + ".fus"))
            {
                _scene[lane][place] = ser.Deserialize(file, null, typeof(SceneContainer)) as SceneContainer;
            }
            _sRlist[lane][place] = new SceneRenderer(_scene[lane][place], "Assets");
            _sOClist[lane][place] = FindByName(childName, _scene[lane][place].Children);
        }

        public static SceneObjectContainer FindByName(string name, IEnumerable<SceneObjectContainer> list)
        {
            foreach (SceneObjectContainer soc in list)
            {
                if (name == soc.Name)
                    return soc;
                if (soc.Children != null)
                {
                    SceneObjectContainer found = FindByName(name, soc.Children);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        // is called once a frame
        public override void RenderAFrame()
        {
           
            switch (_gameState)
            {
                case GameState.Paused:
                    DoPause();
                    break;
                case GameState.InGame:
                    RunGame();
                    break;
                case GameState.RotatingW:
                    DoRotW();
                    break;
                case GameState.RotatingS:
                    DoRotS();
                    break;
                case GameState.GameOver:
                    break;
                case GameState.GameStart:
                    break;
                default:
                    return;
            }
        }

        private void RunGame()
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            
            /*if (_levelSOC != null && (_levelSOC.Transform.Rotation.y >= _twoPi || _levelSOC.Transform.Rotation.y < 0))
            {
                _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y % _twoPi;
            }*/


            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {

                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }

            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            if (Input.Instance.IsKeyDown(KeyCodes.P))
            {
                _guiRender.RenderPause();
                _gameState = GameState.Paused;
            }

            if (Input.Instance.IsKeyDown(KeyCodes.Space))
            {
                if (_currentLane == 0)
                    _currentLane = 1;
                else
                {
                    _currentLane = 0;
                }
            }
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
                ChangeBeeRot(true, 1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle + (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingW;
            }
            if (Input.Instance.IsKey(KeyCodes.S))
            {
                ChangeBeeRot(true, -1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle - (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingS;
            }
            if (Input.Instance.IsKeyDown(KeyCodes.E))
            {
                if (_playerSOC.Transform.Translation.z > 50 && _playerSOC.Transform.Translation.z < 80)
                {
                    if (_punkte > 0)
                    {
                        _playerSOC.Transform.Scale.x = _playerSOC.Transform.Scale.x / 1.1f;
                        _playerSOC.Transform.Scale.y = _playerSOC.Transform.Scale.y / 1.1f;
                        _playerSOC.Transform.Scale.z = _playerSOC.Transform.Scale.z / 1.1f;
                        _groesse = false;
                        _punkte = _punkte - 1;
                        _guiRender.removeNectar(_punkte);
                    }
                    if (_punkte < 0)
                    {
                        _punkte = 0;
                        _voll = false;
                    }

                }
            }

            if (Input.Instance.IsKeyDown(KeyCodes.C))
            {
                if (_sOClist[_currentLane][(int)((_playerSOC.Transform.Translation.z / 1400) * _arrayLength)] != null)
                {
                    _groesse = false;

                    if (_punkte < 5)
                    {
                        if (_groesse == false)
                        {
                            if (_punkte == 4)
                            {
                                _sOClist[_currentLane][(int)((_playerSOC.Transform.Translation.z / 1400) * _arrayLength)].Transform.Scale.y = -5;
                                _sOClist[_currentLane][(int)((_playerSOC.Transform.Translation.z / 1400) * _arrayLength)] = null;
                            }
                            _playerSOC.Transform.Scale.x = _playerSOC.Transform.Scale.x * 1.1f;
                            _playerSOC.Transform.Scale.y = _playerSOC.Transform.Scale.y * 1.1f;
                            _playerSOC.Transform.Scale.z = _playerSOC.Transform.Scale.z * 1.1f;
                            _punkte += 1;
                            _groesse = true;
                            _guiRender.addNectar(_punkte);
                        }
                    }
                }
            }

            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Down))
            {
                ChangeBeeRot(false, 1);
            }

            _levelSR.Render(RC);
            _stockSR.Render(RC);
            _playerSR.Render(RC);
            for (int lC = 0; lC < _lanesArray; lC++)
            {
                if (lC == _currentLane)
                {
                    for (int rCount = 0; rCount < _sRlist[lC].Length; rCount++)
                    {
                        if (_sRlist[lC][rCount] != null)
                        {
                            _sRlist[lC][rCount].Render(RC);
                        }

                    }
                }

            }
            _guiRender.RenderIngame();
            Present();
        }

        private void DoPause()
        {
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {

                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            if (Input.Instance.IsKeyDown(KeyCodes.P))
            {
                _guiRender.DeletePause();
                _gameState = GameState.InGame;
            }
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);

            _levelSR.Render(RC);
            _stockSR.Render(RC);
            _playerSR.Render(RC);
            for (int lC = 0; lC < _lanesArray; lC++)
            {
                if (lC == _currentLane)
                {
                    for (int rCount = 0; rCount < _sRlist[lC].Length; rCount++)
                    {
                        if (_sRlist[lC][rCount] != null)
                        {
                            _sRlist[lC][rCount].Render(RC);
                        }

                    }
                }

            }
            _guiRender.RenderIngame();
            Present();
        }

        private void DoRotW()
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            _guiRender.RenderIngame();
            /*if (_levelSOC != null)
            {
                if (_levelSOC.Transform.Rotation.y < 0 || _yAngle < 0)
                {
                    _levelSOC.Transform.Rotation.y = _twoPi - _levelSOC.Transform.Rotation.y;
                    _yAngle = _twoPi - _yAngle;
                }
                if (_levelSOC.Transform.Rotation.y >= _twoPi || _yAngle >= _twoPi)
                {
                    _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y - _twoPi;
                    _yAngle = _yAngle - _twoPi;
                }
            }*/
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {

                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            //---------------------------------
            /*if (_newRot > _twoPi && _yAngle > _newRot % _twoPi)
            {
                _yAngle += 0.001f;
                //_yAngle += 0.5f * (float)Time.Instance.DeltaTime;
            }
            else if (_yAngle < _newRot % _twoPi)
            {
                _yAngle += 0.001f;
                //_yAngle += 0.5f * (float)Time.Instance.DeltaTime;
            }*/
            if ((_yAngle < _newRot && _newRot < _twoPi) || (_yAngle < _newRot - _twoPi && _newRot > _twoPi))
            {
                if (_yAngle + 0.001f > _twoPi)
                {
                    _yAngle = (_yAngle + 0.020f) - _twoPi;
                }
                else
                {
                    _yAngle = (_yAngle + 0.020f);
                }
            }
            else
            {
                if (_newRot > _twoPi)
                {
                    _yAngle = _newRot - _twoPi;
                }
                else
                {
                    _yAngle = _newRot;
                }
                if (_currentLane + 1 >= _lanesArray)
                {
                    _currentLane = _currentLane - _lanesArray;
                }
                else
                {
                    _currentLane = _currentLane + 1;
                }
                //_currentLane = (((_currentLane + 1) % _lanesArray) + _lanesArray) % _lanesArray;
                _gameState = GameState.InGame;
            }


            if (_levelSOC != null && _stockSOC != null)
            {
                _levelSOC.Transform.Rotation.y = _yAngle;
                _stockSOC.Transform.Rotation.y = _yAngle;
            }
            _levelSR.Render(RC);
            _stockSR.Render(RC);
            _playerSR.Render(RC);

            
            Present();
        }

        private void DoRotS() //Beim auf Null gehen bugts noch TODO
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            _guiRender.RenderIngame();
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            /*if (_levelSOC != null)
            {
                if (_levelSOC.Transform.Rotation.y < 0 || _yAngle < 0)
                {
                    _levelSOC.Transform.Rotation.y = _twoPi - _levelSOC.Transform.Rotation.y;
                    _yAngle = _twoPi - _yAngle;
                }
                if (_levelSOC.Transform.Rotation.y >= _twoPi || _yAngle >= _twoPi)
                {
                    _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y - _twoPi;
                    _yAngle = _yAngle - _twoPi;
                }
            }*/
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {

                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            //---------------------------------
            /* if (_newRot < 0 && ( _yAngle < _twoPi - _newRot || _yAngle == 0 ))
             {
                 if (_yAngle - 0.5f*(float) Time.Instance.DeltaTime < 0)
                 {
                     _yAngle = _twoPi - (_yAngle - 0.5f*(float) Time.Instance.DeltaTime);
                 }
                 else
                 {
                     _yAngle = (_yAngle - 0.5f * (float)Time.Instance.DeltaTime);
                 }
             }
             else */
            if ((_yAngle > _newRot && _newRot > 0) || (_yAngle > _twoPi - _newRot && _newRot < 0))
            {
                if (_yAngle - 0.001f < 0)
                {
                    _yAngle = _twoPi - (_yAngle - 0.020f);
                }
                else
                {
                    _yAngle = (_yAngle - 0.020f);
                }
            }
            else
            {
                if (_newRot < 0)
                {
                    _yAngle = _twoPi - _newRot;
                }
                else
                {
                    _yAngle = _newRot;
                }
                if (_currentLane - 1 < 0)
                {
                    _currentLane = _currentLane + _lanesArray;
                }
                else
                {
                    _currentLane = _currentLane - 1;
                }
                _gameState = GameState.InGame;
            }


            if (_levelSOC != null && _stockSOC != null)
            {
                _levelSOC.Transform.Rotation.y = _yAngle;
                _stockSOC.Transform.Rotation.y = _yAngle;
            }
            _levelSR.Render(RC);
            _stockSR.Render(RC);
            _playerSR.Render(RC);

            
            Present();
        }


        private void ChangeBeeRot(bool what, int dir)
        {
            if (what != _rotChanged || dir != 0)
            {
                if (what == true)
                {
                    _playerSOC.Transform.Rotation.y = 1.2f * dir;
                }
                else
                {
                    _playerSOC.Transform.Rotation.y = 0.0f;
                }

                _rotChanged = what;
            }
            return;
        }

        // is called when the window was resized
        public override void Resize()
        {
            RC.Viewport(0, 0, _screenWidth, _screenHeight / 9 * 2);
            var aspectRatio = _screenWidth / (float)_screenHeight / 9 * 2;
            //RC.Projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 280, 10000);
            RC.Projection = float4x4.CreateOrthographic((float)(_screenWidth * 1.2), _screenHeight / 9 * 2, 2, 100000);
            _guiRender.Refresh();
        }

        public static void Main()
        {
            var app = new BeeTheGame();
            app.Run();
        }
    }
}