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
    public enum GameState { Paused, InGame, RotatingW, RotatingS, GameOver, GameStart, GameHelp }; // Die verschiedenen Stati, die das Spiel haben kann

    public class BeeTheGame : RenderCanvas
    {
        private float _yAngle; // Der Winkel des Levels
        private float _xPos; // Die Position des Spielers: X
        private float _yPos; // Die Position des Spielers: Y
        private bool _rotChanged = false; // Hat sich die Rotation des Spielers ge�ndert? (Lanewechsel)
        private bool _groesse = false;
        private int _punkte = 0; // Anzahl des gesammelten Nektar (max 5)
        private float _aufloesung = 1.2f; // Standard Aufl�sung,bestimmt die Breite des Spielfeldes
        private IAudioStream _ton_weg; // Implementierte Sound
        private IAudioStream _ton_sammeln; 
        private IAudioStream _ton_abgeben;
        private IAudioStream _ton_fliegen;
        private IAudioStream _ton_hintergrund;
        private int _score = 0; // Der gesammt Punktestand
        private int _grenze = 1; //

        private String[] assetsStrings = { "blume_blau", "blume_gold", "blume_lila" }; // Datei-Namen der .fus-Assets f�r die Blumen
        private String[] assetsContStrings = { "blume_blau_container", "blume_gold_container", "blume_lila_container" }; // Container-Namen der Assets f�r die Blumen

        private float _newRot; // Den Winkel der das Level einnehmen soll beim Lanewechsel

        private int _arrayLength = 10; // Anzahl der Grid-Position
        private int _lanesArray = 6; // Anzahl der Lanes

        private int _currentLane = 0; // Momentane Lane des Spielers

        //Automatische Erkennung der Bildschirmsufl�sung
        private int _screenWidth = 800; // Nomierte Aufl�sung bzw. bezugspunkt f�r Aufl�sung (Breite) 
        private int _screenHeight = 600; // Nomierte Aufl�sung bzw. bezugspunkt f�r Aufl�sung (H�he)
        private int _screenWidthAspect = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width; // Aktuelle Aufl�sung (Breite)
        private int _screenHeightAspect = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; // Aktuelle Aufl�sung (Breite)

        private float _twoPi = (float)Math.Round(MathHelper.TwoPi, 6); // Wert f�r 2pi abgerundet auf 6 stellen um Vergleich zu erleichtern

        public GameState _gameState; // Momentaner Spielzustand 

        //Importierte 3D-Objekte (Level)
        private SceneObjectContainer _levelSOC; 
        private SceneRenderer _levelSR;
        private SceneContainer _levelSC;
        //Importierte 3D-Objekte (Bienenstock)
        private SceneObjectContainer _stockSOC;
        private SceneRenderer _stockSR;
        private SceneContainer _stockSC;
        //Importierte 3D-Objekte (Spieler)
        private SceneObjectContainer _playerSOC;
        private SceneRenderer _playerSR;
        private SceneContainer _playerSC;
        //Importierte 3D-Objekte (Blumen)
        private SceneObjectContainer[][] _sOClist;
        private SceneRenderer[][] _sRlist;
        private SceneContainer[][] _scene;
        private int[] _objCountOnLane; // Anzahl der Blumen auf der Lane
        //Importierte 3D-Objekte (Gegner)
        private SceneObjectContainer[][] _enemySOClist;
        private SceneRenderer[][] _enemySRlist;
        private SceneContainer[][] _enemyScene;
        private bool[] _enemyMove; // Bewegungszustand des Gegners

        private GUIRender _guiRender; // GUI Renderer
        public override void Init()
        {
            // Initialisierung der Sound-Dateien (.mp3)
            _ton_sammeln = Audio.Instance.LoadFile("Assets/schmotzer1.mp3", true);
            _ton_weg = Audio.Instance.LoadFile("Assets/Klick2.mp3", true);
            _ton_abgeben = Audio.Instance.LoadFile("Assets/slash1.mp3", true);
            _ton_fliegen = Audio.Instance.LoadFile("Assets/biene_LOOP2.mp3", true);
            _ton_hintergrund = Audio.Instance.LoadFile("Assets/backgroundmusic_tech.mp3",true);
            // Initialisierung der Fenstergr��e
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
            using (var filePlayer = File.OpenRead(@"Assets/Biene.fus"))
            {
                _playerSC = seri.Deserialize(filePlayer, null, typeof(SceneContainer)) as SceneContainer;
            }
            _playerSR = new SceneRenderer(_playerSC, "Assets");
            _playerSOC = FindByName("Biene", _playerSC.Children);
            _playerSOC.Transform.Scale = _playerSOC.Transform.Scale / 25;
            _playerSOC.Transform.Rotation.z = _twoPi/2;
            _playerSOC.Transform.Rotation.x = _twoPi/2;

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
            _enemyMove = new bool[_arrayLength];
            for (int c5 = 0; c5 < _enemyMove.Length; c5++)
            {
                _enemyMove[c5] = true;
            }
            #endregion
           
            // Erzeugung der Blumen auf den Lanes
            for (int aCount = 0; aCount < 15; aCount++)
            {
                SpawnFlower();
            }
            // Erzeugung der Gegner auf den Lanes
            for (int aCount = 0; aCount < _lanesArray + _lanesArray/2; aCount++)
            {
                SpawnEnemy();
            }
            
            RC.ClearColor = new float4(0.1f, 0.1f, 0.5f, 1);
            _yPos = 100;
            _xPos = 150;
            _gameState = GameState.GameStart; // Spiel auf Startbildschirm setzen

            _guiRender = new GUIRender(RC, this);
            _guiRender.StartMenue(); // Rendern des Startmen�s

            _ton_hintergrund.Play(true); // Abspielen der Hintergrundmusik und Lautst�rke setzen
            _ton_fliegen.Volume = 100;
            _ton_fliegen.Play(true); // Abspielen des Fl�gelschlags und und Lautst�rke setzen 
            _ton_fliegen.Volume = 0;
        }

        // Erzeugt eine Blume auf zuf�lliger Lane an einer zuf�lligen Position 
        private void SpawnFlower()
        {
            Random rnd = new Random();
            int randomLane = rnd.Next(_lanesArray-1);
            int randomGrid = rnd.Next(_arrayLength-1);
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
            _sOClist[randomLane][randomGrid].Transform.Translation.x = -25; // Tiefe
            _sOClist[randomLane][randomGrid].Transform.Translation.y = 45; // H�he
            _sOClist[randomLane][randomGrid].Transform.Translation.z = 1400 * (float)(randomGrid + 1) / _arrayLength;
            _sOClist[randomLane][randomGrid].Transform.Rotation.y = 90;
            _objCountOnLane[randomLane] += 1;
        }

        // Erzeugt eine Gegner auf zuf�lliger Lane an einer zuf�lligen Position mit zuf�lliger H�he
        private void SpawnEnemy()
        {
            Random rnd = new Random();
            int randomLane = rnd.Next(_lanesArray-1);
            int randomGrid = rnd.Next(_arrayLength-1);
            //Lane und Pos check
            while (_enemySOClist[randomLane][randomGrid] != null)
            {
                randomLane = rnd.Next(_lanesArray - 1);
                randomGrid = rnd.Next(_arrayLength - 1);
            }
            loadC4DEnemy("spinne_final", randomLane, randomGrid, "Null Body");
            _enemySOClist[randomLane][randomGrid].Transform.Scale = _enemySOClist[randomLane][randomGrid].Transform.Scale / 15;
            _enemySOClist[randomLane][randomGrid].Transform.Translation.x = 20; // Tiefe
            _enemySOClist[randomLane][randomGrid].Transform.Translation.y = 70 + rnd.Next(230); //H�he
            _enemySOClist[randomLane][randomGrid].Transform.Translation.z = 1400 * (float)(randomGrid + 1) / _arrayLength;
            _enemySOClist[randomLane][randomGrid].Transform.Rotation.z = _twoPi/4 - _twoPi/2;
            _enemySOClist[randomLane][randomGrid].Transform.Rotation.x = _twoPi / 4 + _twoPi / 2 + _twoPi / 12;
        }

        // L�dt eine Gegner aus .fus-Datei
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

        // L�dt eine Blume aus .fus-Datei
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

        // Wird ben�tigt um 3D-Object anzusprechen
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
            // Gamestate machine
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
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            // Wenn Maus das Fenster verl�sst wird es zugeklappt und Ton abgestellt + Spiel pausiert
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
                _ton_hintergrund.Volume = 0;
                _guiRender.RenderPause();
                _gameState = GameState.Paused;
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
                _ton_hintergrund.Volume = 100;
                _guiRender.DeletePause();
                _gameState = GameState.InGame;
            }
            // Spieler Position wird aktuallisiert
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            // Hit-Detection with Enemy
            if (_enemySOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z - 80) / 1400) * _arrayLength)] != null)
            {
                float enemyPosY = _enemySOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z - 80) / 1400) * _arrayLength)].Transform.Translation.y;
                float enemyPosZ = _enemySOClist[_currentLane][(int)(((_playerSOC.Transform.Translation.z - 80) / 1400) * _arrayLength)].Transform.Translation.z;
                float playerPosY = _playerSOC.Transform.Translation.y;
                float playerPosZ = _playerSOC.Transform.Translation.z;
                if (enemyPosY - 20.0f <= playerPosY && enemyPosY + 20.0f >= playerPosY && enemyPosZ - 20.0f <= playerPosZ && enemyPosZ + 20.0f >= playerPosZ)
                {
                    if(_punkte >0)
                    {
                        _punkte -= 1;
                        _guiRender.removeNectar(_punkte);
                        _playerSOC.Transform.Scale.x = _playerSOC.Transform.Scale.x / 1.1f;
                        _playerSOC.Transform.Scale.y = _playerSOC.Transform.Scale.y / 1.1f;
                        _playerSOC.Transform.Scale.z = _playerSOC.Transform.Scale.z / 1.1f;
                        _groesse = false;
                        _ton_abgeben.Play();
                    }
                   
                }
            }
            if (Input.Instance.IsKeyDown(KeyCodes.P)) // Pausiert das Spiel
            {
                _guiRender.RenderPause();
                _gameState = GameState.Paused;
            }
            if (Input.Instance.IsKey(KeyCodes.Up) && _yPos < Height+35)
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

            if (Input.Instance.IsKey(KeyCodes.Down) && _yPos > 90)
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

            if (Input.Instance.IsKey(KeyCodes.W)) // && _objCountOnLane[_currentLane] == 0 // Auskommentiertes sorgt f�r freien Lane-Wechsel
            { 
                ChangeBeeRot(true, -1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle + (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingW;
                _ton_fliegen.Play();
            }
            if (Input.Instance.IsKey(KeyCodes.S)) // && _objCountOnLane[_currentLane] == 0
            {
                ChangeBeeRot(true, 1);
                _yAngle = _levelSOC.Transform.Rotation.y;
                _newRot = (float)Math.Round(_yAngle - (_twoPi / _lanesArray), 6);
                _gameState = GameState.RotatingS;
                _ton_fliegen.Play();
            }
            // Lautst�rke des Fl�gelschlag bei Bewegung des Spielers auf 100 setzten und anderenfalls auf 0 
            if (Input.Instance.IsKey(KeyCodes.Right) || Input.Instance.IsKey(KeyCodes.Left) || Input.Instance.IsKey(KeyCodes.Up) || Input.Instance.IsKey(KeyCodes.Down))
            {
                _ton_fliegen.Volume = 100;
            }
            else
            {
                _ton_fliegen.Volume = 0;
            }
            // Interaktion beim Ablegen des Nektars beim Bienenstock 
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
            // Interaktion beim Einsammeln des Nektars bei der Blume 
            if (Input.Instance.IsKeyDown(KeyCodes.Space) && _playerSOC.Transform.Translation.z >= 90)
            {
                // Hit Detektion mit Blume
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
            // Rendern der Basiselemente (Level,Bienenstock,Spieler)
            _levelSR.Render(RC);
            _stockSR.Render(RC);
            _playerSR.Render(RC);
            // Rendern der Gegner und Blumen der momentanen Lane
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
                            // Gegnerbewegung
                            if (_enemySOClist[lC][rCount].Transform.Translation.y == 255)
                            {
                                _enemyMove[rCount] = false;
                            }
                            if (_enemySOClist[lC][rCount].Transform.Translation.y == 80)
                            {
                                _enemyMove[rCount] = true;
                            }
                            if (_enemyMove[rCount])
                            {
                                _enemySOClist[lC][rCount].Transform.Translation.y = _enemySOClist[lC][rCount].Transform.Translation.y + _grenze;
                            }
                            else
                            {
                                _enemySOClist[lC][rCount].Transform.Translation.y = _enemySOClist[lC][rCount].Transform.Translation.y - _grenze;
                            }
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
            // Wenn Maus das Fenster verl�sst wird es zugeklappt und Ton abgestellt
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
                _ton_hintergrund.Volume = 0;
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
                _ton_hintergrund.Volume = 100;
            }
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            _guiRender.RenderIngame();
            Present();
        }
        private void DoHelp()
        { 
            // Wenn Maus das Fenster verl�sst wird es zugeklappt und Ton abgestellt
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
                _ton_hintergrund.Volume = 0;
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
                _ton_hintergrund.Volume = 100;
            }
            if (Input.Instance.IsKeyDown(KeyCodes.P)) // Kehrt ins Spiel zur�ck
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
            // Wenn Maus das Fenster verl�sst wird es zugeklappt und Ton abgestellt
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
                _ton_hintergrund.Volume = 0;
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
                _ton_hintergrund.Volume = 20;   
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

        private void DoRotW() // Rotiert das Level vorw�rts um gewissen Winkel (als Animationsersatz)
        {
            _guiRender.RenderIngame();
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            // Wenn Maus das Fenster verl�sst wird es zugeklappt
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            // Winkelabfrage
            if ((_yAngle < _newRot && _newRot < _twoPi) || (_yAngle < _newRot - _twoPi && _newRot > _twoPi))
            {
                if (_yAngle + 0.001f > _twoPi)
                {
                    _yAngle = (_yAngle + 0.02f) - _twoPi;
                }
                else
                {
                    _yAngle = (_yAngle + 0.02f);
                }
            }
            else
            {
                // Rotation
                if (_newRot > _twoPi)
                {
                    _yAngle = _newRot - _twoPi;
                }
                else
                {
                    _yAngle = _newRot;
                }
                // Setzen der neuen momentanen Lane
                if (_currentLane + 1 >= _lanesArray)
                {
                    _currentLane = (_currentLane + 1) - _lanesArray;
                }
                else
                {
                    _currentLane = _currentLane + 1;
                }
                NewLaneEnemyMove();
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

        private void DoRotS() // Rotiert das Level r�ckw�rts um gewissen Winkel (als Animationsersatz)
        {
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            _guiRender.RenderIngame();
            RC.ModelView = float4x4.LookAt(150, 160, 800, 0, 145, 800, 0, 1, 0);
            if (_playerSOC != null)
            {
                _playerSOC.Transform.Translation.z = _xPos;
                _playerSOC.Transform.Translation.y = _yPos;
            }
            // Wenn Maus das Fenster verl�sst wird es zugeklappt
            if (Control.MousePosition.Y > _screenHeight / 9 * 2)
            {
                SetWindowSize(0, 0, true, 0, 0);
            }
            else
            {
                SetWindowSize(_screenWidth + 20, _screenHeight / 9 * 2, true, 0, 0);
            }
            if ((_yAngle > _newRot && _newRot > 0) || (_yAngle > _twoPi - _newRot && _newRot < 0))
            {
                if (_yAngle - 0.001f < 0)
                {
                    _yAngle = _twoPi - (_yAngle - 0.02f);
                }
                else
                {
                    _yAngle = (_yAngle - 0.02f);
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
                    _currentLane = (_currentLane - 1) + _lanesArray;
                }
                else
                {
                    _currentLane = _currentLane - 1;
                }
                NewLaneEnemyMove();
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


        private void ChangeBeeRot(bool what, int dir) // �ndert die Rotation als Animationsersatz bis n�chste Bewegung
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

        private void NewLaneEnemyMove() // Initialsiert die Anfangsbewegung der Gegner
        {
            for (int c5 = 0; c5 < _enemyMove.Length; c5++)
            {
                _enemyMove[c5] = true;
            }
        }

        // is called when the window was resized
        public override void Resize()
        {
            // Anpassung der Breite an die verschiedene Aufl�sungen (1680,1280,1366,1024)
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
            // Anpassung der orthogonalen Ansicht an die verschiedenen Aufl�sungen
            RC.Viewport(0, 0, _screenWidth, _screenHeight / 9 * 2);
            RC.Projection = float4x4.CreateOrthographic((float)(_screenWidth * _aufloesung), _screenHeight / 9 * 2, 2, 100000);
            // Aktualisierung der GUI 
            _guiRender.Refresh();
        }

        public static void Main()
        {
            var app = new BeeTheGame();            
            app.Run();
        }
    }
}