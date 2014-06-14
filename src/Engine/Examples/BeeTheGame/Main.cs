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

namespace Examples.BeeTheGame
{
    public enum GameState { Paused, InGame, RotatingW, RotatingS, GameOver, GameStart };

    public class BeeTheGame : RenderCanvas
    {
        private float _yAngle;
        private float _xPos;
        private float _yPos;
        private bool _rotChanged = false;

        private float _newRot;

        private int _arrayLength = 10;
        private int _lanesArray = 6;

        private int _currentLane = 0;

        private int _screenWidth = 800;
        private int _screenHeight = 600;
        private int _screenWidthAspect = 1680;
        private int _screenHeightAspect = 945;

        private float _twoPi = (float) Math.Round(MathHelper.TwoPi, 6);

        private GameState _gameState;

        private SceneObjectContainer _levelSOC;
        private SceneRenderer _levelSR;
        private SceneContainer _levelSC;

        private SceneObjectContainer _playerSOC;
        private SceneRenderer _playerSR;
        private SceneContainer _playerSC;

        private SceneObjectContainer[][] _sOClist;
        private SceneRenderer[][] _sRlist;
        private SceneContainer[][] _scene;
        // private int allObjCount = 0;

        public override void Init()
        {
            _screenWidth = Screen.PrimaryScreen.Bounds.Width;
            _screenWidthAspect = _screenWidthAspect/1680;
            _screenHeight = Screen.PrimaryScreen.Bounds.Height;
            _screenHeightAspect = _screenHeight / 945;
            SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0 + 2, 0);

            #region LevelInit
            var seri = new Serializer();
            using (var fileLevel = File.OpenRead(@"Assets/Bienenstock.fus"))
            {
                _levelSC = seri.Deserialize(fileLevel, null, typeof(SceneContainer)) as SceneContainer;
            }
            _levelSR = new SceneRenderer(_levelSC, "Assets");
            _levelSOC = FindByName("bienenstock", _levelSC.Children);
            #endregion
            #region PlayerInit
            using (var filePlayer = File.OpenRead(@"Assets/blume_lila.fus"))
            {
                _playerSC = seri.Deserialize(filePlayer, null, typeof(SceneContainer)) as SceneContainer;
            }
            _playerSR = new SceneRenderer(_playerSC, "Assets");
            _playerSOC = FindByName("blume_lila_container", _playerSC.Children);
            _playerSOC.Transform.Scale = _playerSOC.Transform.Scale / 5;
            #endregion

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



            /*
             _sOClist = new SceneObjectContainer[_lanesArray][_arrayLength];
            _sRlist = new SceneRenderer[_lanesArray][_arrayLength];
            _scene = new SceneContainer[_lanesArray][_arrayLength];
             */

            loadC4D("Bienenstock", 0, 5, "bienenstock");
            _sOClist[0][5].Transform.Scale = _sOClist[0][5].Transform.Scale / 12;
            _sOClist[0][5].Transform.Translation.x = 25;
            _sOClist[0][5].Transform.Translation.y = 110; //Höhe?
            _sOClist[0][5].Transform.Translation.z = 1250; //nach Rechts
            _sOClist[0][5].Transform.Rotation.y = 0;


            loadC4D("blume_lila", 0, 3, "blume_lila_container");
            _sOClist[0][3].Transform.Scale = _sOClist[0][3].Transform.Scale / 9;
            _sOClist[0][3].Transform.Translation.x = 25;
            _sOClist[0][3].Transform.Translation.y = 130; //Höhe?
            _sOClist[0][3].Transform.Translation.z = 350; //nach Rechts
            _sOClist[0][3].Transform.Rotation.y = 90;

            loadC4D("blume_gold", 1, 4, "blume_gold_container");
            _sOClist[1][4].Transform.Scale = _sOClist[1][4].Transform.Scale / 9;
            _sOClist[1][4].Transform.Translation.x = 25;
            _sOClist[1][4].Transform.Translation.y = 150; //Höhe?
            _sOClist[1][4].Transform.Translation.z = 480; //nach Rechts
            _sOClist[1][4].Transform.Rotation.y = 90;

            RC.ClearColor = new float4(0.1f, 0.1f, 0.5f, 1);
            _yAngle = 0;
            _xPos = 0;
            _gameState = GameState.InGame;
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
            RC.ModelView = float4x4.LookAt(150 * (_screenWidthAspect / _screenHeightAspect), 180 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 150 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 1, 0);
            /*if (_levelSOC != null && (_levelSOC.Transform.Rotation.y >= _twoPi || _levelSOC.Transform.Rotation.y < 0))
            {
                _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y % _twoPi;
            }*/
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos + 100;
                _playerSOC.Transform.Translation.y = _yPos + 50;
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
                _newRot = (float) Math.Round(_yAngle + (_twoPi / _lanesArray),6);
                _gameState = GameState.RotatingW;
            }
            if (Input.Instance.IsKey(KeyCodes.S))
            {
                ChangeBeeRot(true, -1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float) Math.Round(_yAngle - (_twoPi / _lanesArray),6);
                _gameState = GameState.RotatingS;
            }
            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Down))
            {
                ChangeBeeRot(false, 1);
            }

            _levelSR.Render(RC);
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
            Present();
        }

        private void DoRotW()
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150 * (_screenWidthAspect / _screenHeightAspect), 180 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 150 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 1, 0);
            if (_levelSOC != null)
            {/*
                if (_levelSOC.Transform.Rotation.y < 0 || _yAngle < 0)
                {
                    _levelSOC.Transform.Rotation.y = _twoPi - _levelSOC.Transform.Rotation.y;
                    _yAngle = _twoPi - _yAngle;
                }
                if (_levelSOC.Transform.Rotation.y >= _twoPi || _yAngle >= _twoPi)
                {
                    _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y - _twoPi;
                    _yAngle = _yAngle - _twoPi;
                }*/
            }
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos + 100;
                _playerSOC.Transform.Translation.y = _yPos + 50;
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
                    _yAngle = (_yAngle + 0.001f) - _twoPi;
                }
                else
                {
                    _yAngle = (_yAngle + 0.001f);
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


            if (_levelSOC != null)
            {
                _levelSOC.Transform.Rotation.y = _yAngle;
            }
            _levelSR.Render(RC);
            _playerSR.Render(RC);
           
            Present();
        }

        private void DoRotS() //Beim auf Null gehen bugts noch TODO
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150 * (_screenWidthAspect / _screenHeightAspect), 180 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 150 * _screenHeightAspect, 800 * _screenWidthAspect, 0, 1, 0);
            if (_levelSOC != null)
            {/*
                if (_levelSOC.Transform.Rotation.y < 0 || _yAngle < 0)
                {
                    _levelSOC.Transform.Rotation.y = _twoPi - _levelSOC.Transform.Rotation.y;
                    _yAngle = _twoPi - _yAngle;
                }
                if (_levelSOC.Transform.Rotation.y >= _twoPi || _yAngle >= _twoPi)
                {
                    _levelSOC.Transform.Rotation.y = _levelSOC.Transform.Rotation.y - _twoPi;
                    _yAngle = _yAngle - _twoPi;
                }*/
            }
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos + 100;
                _playerSOC.Transform.Translation.y = _yPos + 50;
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
                    _yAngle = _twoPi - (_yAngle - 0.001f);
                }
                else
                {
                    _yAngle = (_yAngle - 0.001f);
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

            if (_levelSOC != null)
            {
                _levelSOC.Transform.Rotation.y = _yAngle;
            }
            _levelSR.Render(RC);
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
            RC.Viewport(0, 0, Width, Height);
            var aspectRatio = Width / (float)Height;
            //RC.Projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 280, 10000);
            RC.Projection = float4x4.CreateOrthographic(Width, Height, 5, 100000);
        }

        public static void Main()
        {
            var app = new BeeTheGame();
            app.Run();
        }
    }
}
