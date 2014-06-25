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
    public enum GameState { Paused, InGame, RotatingW, RotatingS, GameOver, GameStart, GameHelp };

    public class BeeTheGame : RenderCanvas
    {
        private float _yAngle;
        private float _xPos;
        private float _yPos;
        private bool _rotChanged = false;
        private bool _groesse = false;
        private int _punkte = 0;
        private float _aufloesung = 1.2f;
        private IAudioStream _ton_weg;
        private IAudioStream _ton_sammeln;
        private IAudioStream _ton_abgeben;
        private IAudioStream _ton_fliegen;
        private IAudioStream _ton_hintergrund;
        private int _score = 0;

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

        public GameState _gameState;

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
        private int[] _objCountOnLane;

        private SceneObjectContainer[][] _enemySOClist;
        private SceneRenderer[][] _enemySRlist;
        private SceneContainer[][] _enemyScene;

        //GUI Stuff
        private GUIRender _guiRender;
        public override void Init()
        {
            _ton_sammeln = Audio.Instance.LoadFile("Assets/schmotzer1.mp3",false);
            _ton_weg = Audio.Instance.LoadFile("Assets/Klick2.mp3", true);
            _ton_abgeben = Audio.Instance.LoadFile("Assets/slash1.mp3", false);
            _ton_fliegen = Audio.Instance.LoadFile("Assets/biene_LOOP2.mp3", true);
            _ton_hintergrund = Audio.Instance.LoadFile("Assets/backgroundmusic.mp3",true);

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
            using (var filePlayer = File.OpenRead(@"Assets/spinne_final.fus"))
            {
                _playerSC = seri.Deserialize(filePlayer, null, typeof(SceneContainer)) as SceneContainer;
            }
            _playerSR = new SceneRenderer(_playerSC, "Assets");
            _playerSOC = FindByName("Null Body", _playerSC.Children);
            _playerSOC.Transform.Scale = _playerSOC.Transform.Scale / 20;
            _playerSOC.Transform.Rotation.z = _twoPi/4;
            _playerSOC.Transform.Rotation.x = _twoPi/4;

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

            //Enemy
            _enemySOClist = new SceneObjectContainer[_lanesArray][];
            _enemySRlist = new SceneRenderer[_lanesArray][];
            _enemyScene = new SceneContainer[_lanesArray][];

            for (int c1 = 0; c1 < _enemySOClist.Length; c1++)
            {
                _enemySOClist[c1] = new SceneObjectContainer[_arrayLength];
            }
            for (int c2 = 0; c2 < _enemySOClist.Length; c2++)
            {
                _enemySRlist[c2] = new SceneRenderer[_arrayLength];
            }
            for (int c3 = 0; c3 < _enemySOClist.Length; c3++)
            {
                _enemyScene[c3] = new SceneContainer[_arrayLength];
            }

            _objCountOnLane = new int[_lanesArray];
            for (int c4 = 0; c4 < _objCountOnLane.Length; c4++)
            {
                _objCountOnLane[c4] = 0;
            }
            #endregion

            for (int aCount = 0; aCount < 15; aCount++)
            {
                SpawnFlower();
            }
            //Enemy
            for (int aCount = 0; aCount < _lanesArray + _lanesArray/2; aCount++)
            {
                SpawnEnemy();
            }
            RC.ClearColor = new float4(0.1f, 0.1f, 0.5f, 1);
            _yPos = 100;
            _xPos = 150;
            _gameState = GameState.GameStart;

            //GUI Stuff
            _guiRender = new GUIRender(RC, this);

            _guiRender.StartMenue();

            _ton_hintergrund.Play(true);
            _ton_fliegen.Volume = 100;
            _ton_fliegen.Play(true);
            _ton_fliegen.Volume = 0;
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
            _objCountOnLane[randomLane] += 1;
        }

        private void SpawnEnemy()
        {
            Random rnd = new Random();
            int randomLane = rnd.Next(_lanesArray);
            int randomGrid = rnd.Next(_arrayLength);
            //Lane und Pos check
            while (_enemySOClist[randomLane][randomGrid] != null)
            {
                randomLane = rnd.Next(_lanesArray - 1);
                randomGrid = rnd.Next(_arrayLength - 1);
            }
            loadC4DEnemy("spinne_final", randomLane, randomGrid, "Null Body");
            _enemySOClist[randomLane][randomGrid].Transform.Scale = _enemySOClist[randomLane][randomGrid].Transform.Scale / 15;
            _enemySOClist[randomLane][randomGrid].Transform.Translation.x = -25;
            _enemySOClist[randomLane][randomGrid].Transform.Translation.y = 145; //Höhe?//ok?
            _enemySOClist[randomLane][randomGrid].Transform.Translation.z = 1400 * (float)(randomGrid + 1) / _arrayLength;
            _enemySOClist[randomLane][randomGrid].Transform.Rotation.z = _twoPi/4;
            _enemySOClist[randomLane][randomGrid].Transform.Rotation.x = _twoPi/4;
        }

        private void loadC4DEnemy(string name, int lane, int place, string childName)
        {
            var ser = new Serializer();

            using (var file = File.OpenRead(@"Assets/" + name + ".fus"))
            {
                _enemyScene[lane][place] = ser.Deserialize(file, null, typeof(SceneContainer)) as SceneContainer;
            }
            _enemySRlist[lane][place] = new SceneRenderer(_enemyScene[lane][place], "Assets");
            _enemySOClist[lane][place] = FindByName(childName, _enemyScene[lane][place].Children);
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
                    _ton_fliegen.Volume = 100;
                    DoRotW();
                    break;
                case GameState.RotatingS:
                    _ton_fliegen.Volume = 100;
                    DoRotS();
                    break;
                case GameState.GameOver:
                    break;
                case GameState.GameStart:
                    DoStart();
                    break;
                case GameState.GameHelp:
                    DoHelp();
                    break;
                default:
                    return;
            }
        }

        private void RunGame()
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            //145
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
            if (Input.Instance.IsKey(KeyCodes.Up) && _yPos < Height)
            {
                if (_punkte > 0) 
                {
                    _yPos += (40.0f - (20.0f*_punkte/5)) * (float)Time.Instance.DeltaTime;                     
                }
                else
                {
                    _yPos += 40.0f * (float)Time.Instance.DeltaTime; 
                }
            }

            if (Input.Instance.IsKey(KeyCodes.Down) && _yPos > 70)
            {
                if (_punkte > 0)
                {
                    _yPos -= (40.0f - (20.0f * _punkte / 5)) * (float)Time.Instance.DeltaTime;
                }
                else
                {
                    _yPos -= 40.0f * (float)Time.Instance.DeltaTime;
                }
            }

            if (Input.Instance.IsKey(KeyCodes.Right) && _xPos <= _screenWidth)
            {
                if (_punkte > 0)
                {
                    _xPos += (125.0f - (50.0f * _punkte / 5)) * (float)Time.Instance.DeltaTime;
                }
                else
                {
                    _xPos += 125.0f * (float)Time.Instance.DeltaTime;
                }
            }
            if (Input.Instance.IsKey(KeyCodes.Left) && _xPos >= 60)
            {
                if (_punkte > 0)
                {
                    _xPos -= (125.0f - (50.0f * _punkte / 5)) * (float)Time.Instance.DeltaTime;
                }
                else
                {
                    _xPos -= 125.0f * (float)Time.Instance.DeltaTime;
                }
            }

            if (Input.Instance.IsKey(KeyCodes.W)) // && _objCountOnLane[_currentLane] == 0
            {
                ChangeBeeRot(true, 1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle + (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingW;
                _ton_fliegen.Play();
            }
            if (Input.Instance.IsKey(KeyCodes.S)) // && _objCountOnLane[_currentLane] == 0
            {
                ChangeBeeRot(true, -1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle - (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingS;
                _ton_fliegen.Play();
            }
            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Down))
            {
                _ton_fliegen.Volume = 100;
            }
            else
            {
                _ton_fliegen.Volume = 0;
            }
            if (Input.Instance.IsKeyDown(KeyCodes.Space) && _playerSOC.Transform.Translation.z <= 90)
            {
                if (_playerSOC.Transform.Translation.z > 30)
                {
                    if (_punkte > 0)
                    {
                        _playerSOC.Transform.Scale.x = _playerSOC.Transform.Scale.x / 1.1f;
                        _playerSOC.Transform.Scale.y = _playerSOC.Transform.Scale.y / 1.1f;
                        _playerSOC.Transform.Scale.z = _playerSOC.Transform.Scale.z / 1.1f;
                        _groesse = false;
                        _ton_abgeben.Play();
                        _punkte = _punkte - 1;
                        _score += 1;
                        _guiRender.SetGUIScore(_score);
                        _guiRender.removeNectar(_punkte);
                    }
                }
            }

            if (Input.Instance.IsKeyDown(KeyCodes.Space) && _playerSOC.Transform.Translation.z >= 90)
            {
                if (_sOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z-80) / 1400) * _arrayLength)] != null)
                {
                    _groesse = false;

                    if (_punkte < 5)
                    {
                        if (_groesse == false)
                        {
                            if (_punkte == 4)
                            {
                                _sOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z - 80) / 1400) * _arrayLength)].Transform.Scale.y = -5;
                                _sOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z - 80) / 1400) * _arrayLength)] = null;
                                SpawnFlower();
                                _objCountOnLane[_currentLane] -= 1;
                                _ton_weg.Play();
                            }
                            _playerSOC.Transform.Scale.x = _playerSOC.Transform.Scale.x * 1.1f;
                            _playerSOC.Transform.Scale.y = _playerSOC.Transform.Scale.y * 1.1f;
                            _playerSOC.Transform.Scale.z = _playerSOC.Transform.Scale.z * 1.1f;
                            _punkte += 1;
                            _groesse = true;
                            _ton_sammeln.Play();
                            _guiRender.addNectar(_punkte);
                        }
                    }
                }
            }

            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Right) && Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Right) && Input.Instance.IsKey(KeyCodes.Down))
            {
                ChangeBeeRot(true, 3);
            }
            if (Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Left) && Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Left) && Input.Instance.IsKey(KeyCodes.Down))
            {
                ChangeBeeRot(true, 2);
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
                    for (int rCount = 0; rCount < _enemySRlist[lC].Length; rCount++)
                    {
                        if (_enemySRlist[lC][rCount] != null)
                        {
                            _enemySRlist[lC][rCount].Render(RC);
                        }

                    }
                }

            }
            _guiRender.RenderIngame();
            Present();
        }
        private void DoStart()
        {
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {

                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);

            _guiRender.RenderIngame();
            Present();
        }
        private void DoHelp()
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
            ChangeBeeRot(true, 3);
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
            ChangeBeeRot(true, 3);
            _guiRender.RenderIngame();
            Present();
        }

        private void DoRotW()
        {
            _guiRender.RenderIngame();
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
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
                    if (dir == 1)
                    {
                        _playerSOC.Transform.Rotation.y = 1.2f * dir;
                    }
                    if (dir == -1)
                    {
                        _playerSOC.Transform.Rotation.y = 1.2f * dir;
                    }
                    if (dir == 2)
                    {
                        _playerSOC.Transform.Rotation.y = 3.14f;
                    }
                    if (dir == 3)
                    {
                        _playerSOC.Transform.Rotation.y = 0.0f;
                    }

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

            if (_screenWidth >= 1680)
            {
                _aufloesung = 1.0f;
            }
            if (_screenWidth == 1280)
            {
                _aufloesung = 1.3f;
            }
            if (_screenWidth ==1366)
            {
                _aufloesung = 1.2f;
            }
            if (_screenWidth <= 1024)
            {
                _aufloesung = 1.6f;
            }


            RC.Viewport(0, 0, _screenWidth, _screenHeight / 9 * 2);
            //var aspectRatio = _screenWidth / (float)_screenHeight / 9 * 2;
            //RC.Projection = float4x4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 280, 10000);
            RC.Projection = float4x4.CreateOrthographic((float)(_screenWidth * _aufloesung), _screenHeight / 9 * 2, 2, 100000);
            _guiRender.Refresh();
        }


        public static void Main()
        {
            var app = new BeeTheGame();
            
            app.Run();
        }
    }
}