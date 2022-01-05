//#define DEMO

using GDLibrary;
using GDLibrary.Collections;
using GDLibrary.Components;
using GDLibrary.Components.UI;
using GDLibrary.Core;
using GDLibrary.Core.Demo;
using GDLibrary.Graphics;
using GDLibrary.Inputs;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using GDLibrary.Renderers;
using GDLibrary.Utilities;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace GDApp
{
    public class Main : Game
    {
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// Stores and updates all scenes (which means all game objects i.e. players, cameras, pickups, behaviours, controllers)
        /// </summary>
        private SceneManager sceneManager;

        /// <summary>
        /// Draws all game objects with an attached and enabled renderer
        /// </summary>
        private RenderManager renderManager;

        /// <summary>
        /// Updates and Draws all ui objects
        /// </summary>
        private UISceneManager uiSceneManager;

        /// <summary>
        /// Updates and Draws all menu objects
        /// </summary>
        private MyMenuManager uiMenuManager;

        /// <summary>
        /// Plays all 2D and 3D sounds
        /// </summary>
        private SoundManager soundManager;

        private MyStateManager stateManager;
        private PickingManager pickingManager;

        /// <summary>
        /// Handles all system wide events between entities
        /// </summary>
        private EventDispatcher eventDispatcher;

        /// <summary>
        /// Applies physics to all game objects with a Collider
        /// </summary>
        private PhysicsManager physicsManager;

        /// <summary>
        /// Quick lookup for all textures used within the game
        /// </summary>
        private Dictionary<string, Texture2D> textureDictionary;

        /// <summary>
        /// Quick lookup for all fonts used within the game
        /// </summary>
        private ContentDictionary<SpriteFont> fontDictionary;

        /// <summary>
        /// Quick lookup for all models used within the game
        /// </summary>
        private ContentDictionary<Model> modelDictionary;

        /// <summary>
        /// Quick lookup for all videos used within the game by texture behaviours
        /// </summary>
        private ContentDictionary<Video> videoDictionary;

        //temps
        private Scene activeScene;

        private UITextObject nameTextObj;
        private Collider collider;

        #endregion Fields

        /// <summary>
        /// Construct the Game object
        /// </summary>
        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Set application data, input, title and scene manager
        /// </summary>
        private void InitializeEngine(string gameTitle, int width, int height)
        {
            //set game title
            Window.Title = gameTitle;

            //the most important element! add event dispatcher for system events
            eventDispatcher = new EventDispatcher(this);

            //add physics manager to enable CD/CR and physics
            physicsManager = new PhysicsManager(this);

            //instanciate scene manager to store all scenes
            sceneManager = new SceneManager(this);

            //create the ui scene manager to update and draw all ui scenes
            uiSceneManager = new UISceneManager(this, _spriteBatch);

            //create the ui menu manager to update and draw all menu scenes
            uiMenuManager = new MyMenuManager(this, _spriteBatch);

            //add support for playing sounds
            soundManager = new SoundManager(this);

            //this will check win/lose logic
            stateManager = new MyStateManager(this);

            //picking support using physics engine
            //this predicate lets us say ignore all the other collidable objects except interactables and consumables
            Predicate<GameObject> collisionPredicate =
                (collidableObject) =>
            {
                if (collidableObject != null)
                    return collidableObject.GameObjectType
                    == GameObjectType.Interactable
                    || collidableObject.GameObjectType == GameObjectType.Consumable;

                return false;
            };
            pickingManager = new PickingManager(this, 2, 100, collisionPredicate);

            //initialize global application data
            Application.Main = this;
            Application.Content = Content;
            Application.GraphicsDevice = _graphics.GraphicsDevice;
            Application.GraphicsDeviceManager = _graphics;
            Application.SceneManager = sceneManager;
            Application.PhysicsManager = physicsManager;
            Application.StateManager = stateManager;
            Application.UISceneManager = uiSceneManager;

            //instanciate render manager to render all drawn game objects using preferred renderer (e.g. forward, backward)
            renderManager = new RenderManager(this, new ForwardRenderer(), false, true);

            //instanciate screen (singleton) and set resolution etc
            Screen.GetInstance().Set(width, height, true, true);

            //instanciate input components and store reference in Input for global access
            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Mouse.Position = Screen.Instance.ScreenCentre;
            Input.Gamepad = new GamepadComponent(this);

            //************* add all input components to component list so that they will be updated and/or drawn ***********/

            //add time support
            Components.Add(Time.GetInstance(this));

            //add event dispatcher
            Components.Add(eventDispatcher);

            //add input support
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);

            //add physics manager to enable CD/CR and physics
            Components.Add(physicsManager);

            //add support for picking using physics engine
            Components.Add(pickingManager);

            //add scene manager to update game objects
            Components.Add(sceneManager);

            //add render manager to draw objects
            Components.Add(renderManager);

            //add ui scene manager to update and drawn ui objects
            Components.Add(uiSceneManager);

            //add ui menu manager to update and drawn menu objects
            Components.Add(uiMenuManager);

            //add sound
            Components.Add(soundManager);

            //add state
            Components.Add(stateManager);
        }

        /// <summary>
        /// Not much happens in here as SceneManager, UISceneManager, MenuManager and Inputs are all GameComponents that automatically Update()
        /// Normally we use this to add some temporary demo code in class - Don't forget to remove any temp code inside this method!
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            //if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            //{
            //    //DEMO - raise event
            //    //EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
            //    //    EventActionType.OnPause));

            //    object[] parameters = { nameTextObj };

            //    EventDispatcher.Raise(new EventData(EventCategoryType.UiObject,
            //        EventActionType.OnRemoveObject, parameters));

            //    ////renderManager.StatusType = StatusType.Off;
            //}
            //else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            //{
            //    //DEMO - raise event

            //    object[] parameters = { "main game ui", nameTextObj };

            //    EventDispatcher.Raise(new EventData(EventCategoryType.UiObject,
            //        EventActionType.OnAddObject, parameters));

            //    //renderManager.StatusType = StatusType.Drawn;
            //    //EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
            //    //  EventActionType.OnPlay));
            //}

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                object[] parameters = { "health", 1 };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                    EventActionType.OnHealthDelta, parameters));
            }
            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                object[] parameters = { "health", -1 };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI,
                    EventActionType.OnHealthDelta, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                          EventActionType.OnPause));

                //walking sound effect//
                object[] parameters2 = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters2));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.O))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                    EventActionType.OnPlay));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Q))
            {
                object[] parameters = { "announcement" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.Q))
            {
                object[] parameters = { "announcement" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }
            //testaudio

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.   P))
            {
                object[] parameters = { "testaudio" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay, parameters));
            }

            else if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.P))
            {
                object[] parameters = { "testaudio" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            //walking sounds//
            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.W))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.S))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.D))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            if (Input.Keys.WasJustReleased(Microsoft.Xna.Framework.Input.Keys.A))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }

            //walking sounds//
            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.W))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.A))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.S))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.D))
            {
                object[] parameters = { "Walking_Grass" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.C))
                Application.SceneManager.ActiveScene.CycleCameras();

            if (Input.Keys.WasJustPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                object[] parameters = { "main menu video" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Video,
                    EventActionType.OnPlay, parameters));
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Not much happens in here as RenderManager, UISceneManager and MenuManager are all DrawableGameComponents that automatically Draw()
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }

        /******************************** Student Project-specific ********************************/
        /******************************** Student Project-specific ********************************/
        /******************************** Student Project-specific ********************************/

        #region Student/Group Specific Code

        /// <summary>
        /// Initialize engine, dictionaries, assets, level contents
        /// </summary>
        protected override void Initialize()
        {
            //move here so that UISceneManager can use!
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //data, input, scene manager
            InitializeEngine(AppData.GAME_TITLE_NAME,
                AppData.GAME_RESOLUTION_WIDTH,
                AppData.GAME_RESOLUTION_HEIGHT);

            //load structures that store assets (e.g. textures, sounds) or archetypes (e.g. Quad game object)
            InitializeDictionaries();

            //load assets into the relevant dictionary
            LoadAssets();

            //level with scenes and game objects
            InitializeLevel();

            //add menu and ui
            InitializeUI();

            //TODO - remove hardcoded mouse values - update Screen class to centre the mouse with hardcoded value - remove later
            Input.Mouse.Position = Screen.Instance.ScreenCentre;

            //turn on/off debug info
            //InitializeDebugUI(true, false);

            //to show the menu we must start paused for everything else!
            EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnPause));

            base.Initialize();
        }

        /******************************* Load/Unload Assets *******************************/

        private void InitializeDictionaries()
        {
            textureDictionary = new Dictionary<string, Texture2D>();

            //why not try the new and improved ContentDictionary instead of a basic Dictionary?
            fontDictionary = new ContentDictionary<SpriteFont>();
            modelDictionary = new ContentDictionary<Model>();

            //stores videos
            videoDictionary = new ContentDictionary<Video>();
        }

        private void LoadAssets()
        {
            LoadModels();
            LoadTextures();
            LoadVideos();
            LoadSounds();
            LoadFonts();
        }

        /// <summary>
        /// Loads video content used by UIVideoTextureBehaviour
        /// </summary>
        private void LoadVideos()
        {
            videoDictionary.Add("Assets/Video/main_menu_video");
        }

        /// <summary>
        /// Load models to dictionary
        /// </summary>
        private void LoadModels()
        {
            //notice with the ContentDictionary we dont have to worry about Load() or a name (its assigned from pathname)
            modelDictionary.Add("Assets/Models/sphere");
            modelDictionary.Add("Assets/Models/cube");
            modelDictionary.Add("Assets/Models/teapot");
            modelDictionary.Add("Assets/Models/monkey1");
        }

        /// <summary>
        /// Load fonts to dictionary
        /// </summary>
        private void LoadFonts()
        {
            fontDictionary.Add("Assets/Fonts/ui");
            fontDictionary.Add("Assets/Fonts/menu");
            fontDictionary.Add("Assets/Fonts/debug");
        }

        /// <summary>
        /// Load sound data used by sound manager
        /// </summary>
        private void LoadSounds()
        {
            var soundEffect =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/Walking_Grass");

            //add the new sound effect
            soundManager.Add(new GDLibrary.Managers.Cue(
                "Walking_Grass",
                soundEffect,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            var soundEffect1 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/announcement");

            //add the new sound effect
            soundManager.Add(new GDLibrary.Managers.Cue(
                "announcement",
                soundEffect1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));

            var soundEffect2 =
                Content.Load<SoundEffect>("Assets/Sounds/Effects/testaudio");

            //add the new sound effect
            soundManager.Add(new GDLibrary.Managers.Cue(
                "testaudio",
                soundEffect1,
                SoundCategoryType.Alarm,
                new Vector3(1, 0, 0),
                false));
        }

        /// <summary>
        /// Load texture data from file and add to the dictionary
        /// </summary>
        private void LoadTextures()
        {
            //debug
            textureDictionary.Add("checkerboard", Content.Load<Texture2D>("Assets/Demo/Textures/checkerboard"));
            textureDictionary.Add("mona lisa", Content.Load<Texture2D>("Assets/Demo/Textures/mona lisa"));

            //skybox
            textureDictionary.Add("skybox_front", Content.Load<Texture2D>("Assets/Textures/Skybox/front"));
            textureDictionary.Add("skybox_left", Content.Load<Texture2D>("Assets/Textures/Skybox/left"));
            textureDictionary.Add("skybox_right", Content.Load<Texture2D>("Assets/Textures/Skybox/right"));
            textureDictionary.Add("skybox_back", Content.Load<Texture2D>("Assets/Textures/Skybox/back"));
            textureDictionary.Add("skybox_sky", Content.Load<Texture2D>("Assets/Textures/Skybox/sky"));

            //environment
            textureDictionary.Add("grass", Content.Load<Texture2D>("Assets/Textures/Foliage/Ground/grass1"));
            textureDictionary.Add("crate1", Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate1"));
            textureDictionary.Add("yellow", Content.Load<Texture2D>("Assets/Textures/Props/Crates/yellow"));
            textureDictionary.Add("blue", Content.Load<Texture2D>("Assets/Textures/Props/Crates/blue"));

            //ui
            textureDictionary.Add("ui_progress_32_8", Content.Load<Texture2D>("Assets/Textures/UI/Controls/ui_progress_32_8"));
            textureDictionary.Add("progress_white", Content.Load<Texture2D>("Assets/Textures/UI/Controls/progress_white"));

            //menu
            textureDictionary.Add("mainmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/mainmenu"));
            textureDictionary.Add("audiomenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/audiomenu"));
            textureDictionary.Add("controlsmenu", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/controlsmenu"));
            textureDictionary.Add("exitmenuwithtrans", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/exitmenuwithtrans"));
            textureDictionary.Add("genericbtn", Content.Load<Texture2D>("Assets/Textures/UI/Controls/genericbtn"));

            textureDictionary.Add("hedgegame", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/hedgegame"));
            textureDictionary.Add("ui", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/ui"));
            textureDictionary.Add("uitimer", Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/uitimer"));

            //reticule
            textureDictionary.Add("reticuleOpen",
      Content.Load<Texture2D>("Assets/Textures/UI/Controls/reticuleOpen"));
            textureDictionary.Add("reticuleDefault",
          Content.Load<Texture2D>("Assets/Textures/UI/Controls/reticuleDefault"));
        }

        /// <summary>
        /// Free all asset resources, dictionaries, network connections etc
        /// </summary>
        protected override void UnloadContent()
        {
            //remove all models used for the game and free RAM
            modelDictionary?.Dispose();
            fontDictionary?.Dispose();
            videoDictionary?.Dispose();

            base.UnloadContent();
        }

        /******************************* UI & Menu *******************************/

        /// <summary>
        /// Create a scene, add content, add to the scene manager, and load default scene
        /// </summary>
        private void InitializeLevel()
        {
            float worldScale = 1000;
            activeScene = new Scene("level 1");

            InitializeCameras(activeScene);

            InitializeSkybox(activeScene, worldScale);

            //remove because now we are interested only in collidable things!
            //InitializeCubes(activeScene);
            //InitializeModels(activeScene);

            InitializeCollidables(activeScene, worldScale);

            sceneManager.Add(activeScene);
            sceneManager.LoadScene("level 1");
        }

        /// <summary>
        /// Adds menu and UI elements
        /// </summary>
        private void InitializeUI()
        {
            InitializeGameMenu();
            InitializeGameUI();
        }

        /// <summary>
        /// Adds main menu elements
        /// </summary>
        private void InitializeGameMenu()
        {
            //a re-usable variable for each ui object
            UIObject menuObject = null;

            #region Main Menu

            /************************** Main Menu Scene **************************/
            //make the main menu scene
            var mainMenuUIScene = new UIScene(AppData.MENU_MAIN_NAME);

            /**************************** Background Image ****************************/

            //main background
            var texture = textureDictionary["hedgegame"];
            //get how much we need to scale background to fit screen, then downsizes a little so we can see game behind background
            var scale = _graphics.GetScaleForTexture(texture,
                new Vector2(1.4f, 0.8f));

            menuObject = new UITextureObject("hedgegame",
                UIObjectType.Texture,
                new Transform2D(Screen.Instance.ScreenCentre, scale, 0), //sets position as center of screen
                0,
                new Color(255, 255, 255, 200),
                texture.GetOriginAtCenter(), //if we want to position image on screen center then we need to set origin as texture center
                texture);

            //add ui object to scene
            mainMenuUIScene.Add(menuObject);

            /**************************** Play Button ****************************/

            var btnTexture = textureDictionary["genericbtn"];
            var sourceRectangle
                = new Microsoft.Xna.Framework.Rectangle(0, 0,
                btnTexture.Width, btnTexture.Height);
            var origin = new Vector2(btnTexture.Width / 2.0f, btnTexture.Height / 2.0f);


            var playBtn = new UIButtonObject(AppData.MENU_PLAY_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_PLAY_BTN_POSITION,
                0.9f * Vector2.One, 0),
                0.1f,
                Color.White,
                SpriteEffects.None,
                origin,
                btnTexture,
                null,
                sourceRectangle,
                "Play",
                fontDictionary["menu"],
                Color.Black,
                Vector2.Zero);
            
            
            //demo button color change
            var comp = new UIColorMouseOverBehaviour(Color.Green, Color.White);
            playBtn.AddComponent(comp);
            //exit//

            mainMenuUIScene.Add(playBtn);

            //use a simple/smaller version of the UIButtonObject constructor
            var exitBtn = new UIButtonObject(AppData.MENU_EXIT_BTN_NAME, UIObjectType.Button,
                new Transform2D(AppData.MENU_EXIT_BTN_POSITION,
                0.9f * Vector2.One, 0),
                0.1f,
                Color.Orange,
                origin,
                btnTexture,
                "Exit",
                fontDictionary["menu"],
                Color.Black);

            //demo button color change
            exitBtn.AddComponent(new UIColorMouseOverBehaviour(Color.Red, Color.White));
            mainMenuUIScene.Add(exitBtn);

            #endregion Main Menu

            //add scene to the menu manager
            uiMenuManager.Add(mainMenuUIScene);

            /************************** Controls Menu Scene **************************/

            /************************** Options Menu Scene **************************/

            /************************** Exit Menu Scene **************************/

            //finally we say...where do we start
            uiMenuManager.SetActiveScene(AppData.MENU_MAIN_NAME);
        }

        /// <summary>
        /// Adds ui elements seen in-game (e.g. health, timer)
        /// </summary>
        private void InitializeGameUI()
        {
            //create the scene
            var mainGameUIScene = new UIScene(AppData.UI_SCENE_MAIN_NAME);

            #region Add Health Bar
            /*
            //add a health bar in the centre of the game window
            var texture = textureDictionary["progress_white"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 2, 50);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            //create the UI element
            var healthTextureObj = new UITextureObject("health",
                UIObjectType.Texture,
                new Transform2D(position, new Vector2(2, 0.5f), 0),
                0,
                Color.White,
                origin,
                texture);

            //add a demo time based behaviour - because we can!
            healthTextureObj.AddComponent(new UITimeColorFlipBehaviour(Color.White, Color.Red, 1000));

            //add a progress controller
            healthTextureObj.AddComponent(new UIProgressBarController(5, 10));

            //add the ui element to the scene
            mainGameUIScene.Add(healthTextureObj);
            */
            var texture = textureDictionary["uitimer"];
            var position = new Vector2(_graphics.PreferredBackBufferWidth / 0.99f, 980);
            var origin = new Vector2(texture.Width / 1, texture.Height / 2);

            var healthTextureObj = new UITextureObject("health",
                UIObjectType.Texture,
                new Transform2D(position, new Vector2(2.4f, 0.5f), 0),
                0,
                Color.Orange,
                origin,
                texture);
            //healthTextureObj.AddComponent(new UITimeColorFlipBehaviour(Color.Green, Color.Pink, 2000));
            healthTextureObj.AddComponent(new UiHealthController(100, 100));


            var hudTextureObj = new UITextureObject("ui",
                 UIObjectType.Texture,
                 new Transform2D(new Vector2(6, 900),
                 new Vector2(1.11f, 1),
                 MathHelper.ToRadians(0)),
                 0, Content.Load<Texture2D>("Assets/Textures/UI/Backgrounds/ui"));
            //add the ui element to the scene
            hudTextureObj.Color = Color.White;
            mainGameUIScene.Add(healthTextureObj);
            mainGameUIScene.Add(hudTextureObj);


            #endregion Add Health Bar

            #region Add Text

            //var font = fontDictionary["ui"];
            //var str = "player name";

            //create the UI element
            //nameTextObj = new UITextObject(str, UIObjectType.Text,
            //new Transform2D(new Vector2(50, 50),
            //Vector2.One, 0),
            //0, font, "Brutus Maximus");

            //  nameTextObj.Origin = font.MeasureString(str) / 2;
            //  nameTextObj.AddComponent(new UIExpandFadeBehaviour());

            //add the ui element to the scene
            //mainGameUIScene.Add(nameTextObj);

            #endregion Add Text


            #region Add Reticule
            
            var defaultTexture = textureDictionary["reticuleDefault"];
            var alternateTexture = textureDictionary["reticuleOpen"];
            origin = defaultTexture.GetOriginAtCenter();

            var reticule = new UITextureObject("reticule",
                     UIObjectType.Texture,
                new Transform2D(Vector2.Zero, Vector2.One, 0),
                0,
                Color.White,
                SpriteEffects.None,
                origin,
                defaultTexture,
                alternateTexture,
                new Microsoft.Xna.Framework.Rectangle(0, 0,
                defaultTexture.Width, defaultTexture.Height));

            reticule.AddComponent(new UIReticuleBehaviour());

            mainGameUIScene.Add(reticule);
            
            #endregion Add Reticule


            #region Add Video UI Texture

            ////add a health bar in the centre of the game window
            //texture = textureDictionary["checkerboard"]; //any texture given we will replace it
            //position = new Vector2(300, 300);

            //var video = videoDictionary["main_menu_video"];
            //origin = new Vector2(video.Width / 2, video.Height / 2);

            ////create the UI element
            //var videoTextureObj = new UITextureObject("main menu video",
            //    UIObjectType.Texture,
            //    new Transform2D(position, new Vector2(0.2f, 0.2f), 0),
            //    0,
            //    Color.White,
            //    origin,
            //    texture);

            ////add a video behaviou
            //videoTextureObj.AddComponent(new UIVideoTextureBehaviour(
            //    new VideoCue(video, 0, false)));

            ////add the ui element to the scene
            //mainGameUIScene.Add(videoTextureObj);

            #endregion Add Video UI Texture

            #region Add Scene To Manager & Set Active Scene

            //add the ui scene to the manager
            uiSceneManager.Add(mainGameUIScene);

            //set the active scene
            uiSceneManager.SetActiveScene(AppData.UI_SCENE_MAIN_NAME);

            #endregion Add Scene To Manager & Set Active Scene
        }

        /// <summary>
        /// Adds component to draw debug info to the screen
        /// </summary>
        /*
        private void InitializeDebugUI(bool showDebugInfo, bool showCollisionSkins = true)
        {
            if (showDebugInfo)
            {
                Components.Add(new GDLibrary.Utilities.GDDebug.PerfUtility(this,
                    _spriteBatch, fontDictionary["debug"],
                    new Vector2(40, _graphics.PreferredBackBufferHeight - 80),
                    Color.White));
            }

            if (showCollisionSkins)
                Components.Add(new GDLibrary.Utilities.GDDebug.PhysicsDebugDrawer(this, Color.Red));
        }
        */
        /******************************* Non-Collidables *******************************/

        /// <summary>
        /// Set up the skybox using a QuadMesh
        /// </summary>
        /// <param name="level">Scene Stores all game objects for current...</param>
        /// <param name="worldScale">float Value used to scale skybox normally 250 - 1000</param>
        private void InitializeSkybox(Scene level, float worldScale = 500)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card
            var shader = new BasicShader(Application.Content, true, true);
            //re-use the vertices and indices of the primitive
            var mesh = new QuadMesh();
            //create an archetype that we can clone from
            var archetypalQuad = new GameObject("quad", GameObjectType.Skybox, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;
            //back
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_back";
            clone.Transform.Translate(0, 0, -worldScale / 2.0f);
            clone.Transform.Scale(worldScale, worldScale, 1);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_back_material", shader, Color.White, 1, textureDictionary["skybox_back"])));
            level.Add(clone);

            //left
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_left";
            clone.Transform.Translate(-worldScale / 2.0f, 0, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, 90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_left_material", shader, Color.White, 1, textureDictionary["skybox_left"])));
            level.Add(clone);

            //right
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_right";
            clone.Transform.Translate(worldScale / 2.0f, 0, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, -90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_right_material", shader, Color.White, 1, textureDictionary["skybox_right"])));
            level.Add(clone);

            //front
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_front";
            clone.Transform.Translate(0, 0, worldScale / 2.0f);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(0, -180, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_front_material", shader, Color.White, 1, textureDictionary["skybox_front"])));
            level.Add(clone);

            //top
            clone = archetypalQuad.Clone() as GameObject;
            clone.Name = "skybox_sky";
            clone.Transform.Translate(0, worldScale / 2.0f, 0);
            clone.Transform.Scale(worldScale, worldScale, null);
            clone.Transform.Rotate(90, -90, 0);
            clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("skybox_sky_material", shader, Color.White, 1, textureDictionary["skybox_sky"])));
            level.Add(clone);
        }

        /// <summary>
        /// Initialize the camera(s) in our scene
        /// </summary>
        /// <param name="level"></param>
        private void InitializeCameras(Scene level)
        {
            #region First Person Camera - Non Collidable

            //add camera game object
            var camera = new GameObject(AppData.CAMERA_FIRSTPERSON_NONCOLLIDABLE_NAME, GameObjectType.Camera);

            //add components
            //here is where we can set a smaller viewport e.g. for split screen
            //e.g. new Viewport(0, 0, _graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight)
            camera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));

            //add controller to actually move the noncollidable camera
            camera.AddComponent(new FirstPersonController(0.05f, 0.025f, new Vector2(0.006f, 0.004f)));

            //set initial position
            camera.Transform.SetTranslation(0, 2, 10);

            //add to level
            level.Add(camera);

            #endregion First Person Camera - Non Collidable

            #region Curve Camera - Non Collidable

            //add curve for camera translation
            var translationCurve = new Curve3D(CurveLoopType.Cycle);
            translationCurve.Add(new Vector3(0, 2, 10), 0);
            translationCurve.Add(new Vector3(0, 8, 15), 1000);
            translationCurve.Add(new Vector3(0, 8, 20), 2000);
            translationCurve.Add(new Vector3(0, 6, 25), 3000);
            translationCurve.Add(new Vector3(0, 4, 25), 4000);
            translationCurve.Add(new Vector3(0, 2, 10), 6000);

            //add camera game object
            var curveCamera = new GameObject(AppData.CAMERA_CURVE_NONCOLLIDABLE_NAME, GameObjectType.Camera);

            //add components
            curveCamera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));
            curveCamera.AddComponent(new CurveBehaviour(translationCurve));
            curveCamera.AddComponent(new FOVOnScrollController(MathHelper.ToRadians(2)));

            //add to level
            level.Add(curveCamera);

            #endregion Curve Camera - Non Collidable

            #region First Person Camera - Collidable

            //add camera game object
            camera = new GameObject(AppData.CAMERA_FIRSTPERSON_COLLIDABLE_NAME, GameObjectType.Camera);

            //set initial position - important to set before the collider as collider capsule feeds off this position
            camera.Transform.SetTranslation(30, 10, 30);

            //add components
            camera.AddComponent(new Camera(_graphics.GraphicsDevice.Viewport));

            //adding a collidable surface that enables acceleration, jumping
            var collider = new CharacterCollider(2, 2, true, false);

            camera.AddComponent(collider);
            collider.AddPrimitive(new Capsule(camera.Transform.LocalTranslation,
                Matrix.CreateRotationX(MathHelper.PiOver2), 1, 3.6f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collider.Enable(false, 2);

            //add controller to actually move the collidable camera
            camera.AddComponent(new MyCollidableFirstPersonController(12,
                       0.5f, 0.3f, new Vector2(0.03f, 0.02f)));

            //add to level
            level.Add(camera);

            #endregion First Person Camera - Collidable

            //set the main camera, if we dont call this then the first camera added will be the Main
            level.SetMainCamera(AppData.CAMERA_FIRSTPERSON_COLLIDABLE_NAME);

            //allows us to scale time on all game objects that based movement on Time
            // Time.Instance.TimeScale = 0.1f;
        }

        /******************************* Collidables *******************************/

        /// <summary>
        /// Demo of the new physics manager and collidable objects
        /// </summary>
        private void InitializeCollidables(Scene level, float worldScale = 500)
        {
            InitializeCollidableGround(level, worldScale);
            InitializeCollidableCubes(level);
            InitializeSphere(level);
            InitializeSphere2(level);
            InitializeSphere3(level);
            InitializeCollidableTriangleMeshes(level);
            announcement(level);
            testaudio(level);

            CubeWall1(level);
            CubeWall2(level);
            CubeWall3(level);
            CubeWall4(level);
            CubeWall5(level);

            MazeWall1(level);
            MazeWall2(level);
            MazeWall3(level);
            MazeWall4(level);
            MazeWall5(level);
            MazeWall6(level);
            MazeWall7(level);
            MazeWall8(level);
            MazeWall9(level);
            MazeWall10(level);
            MazeWall11(level);
            MazeWall12(level);
            MazeWall13(level);
            MazeWall14(level);
            MazeWall15(level);
            MazeWall16(level);
            MazeWall17(level);
            MazeWall18(level);
            MazeWall19(level);
            MazeWall20(level);
            MazeWall21(level);
            MazeWall22(level);
            MazeWall23(level);
            MazeWall24(level);
            MazeWall25(level);
            MazeWall26(level);
            MazeWall27(level);
            MazeWall28(level);
            MazeWall29(level);
            MazeWall30(level);
            MazeWall31(level);
            MazeWall32(level);
            MazeWall33(level);
            MazeWall34(level);
            MazeWall35(level);
            MazeWall36(level);
            MazeWall37(level);
            MazeWall38(level);
            MazeWall39(level);
            MazeWall40(level);
            MazeWall41(level);
            MazeWall42(level);
            MazeWall43(level);
            MazeWall44(level);
            MazeWall45(level);
            MazeWall46(level);
            MazeWall47(level);
            MazeWall48(level);
            MazeWall49(level);
            MazeWall50(level);
            MazeWall51(level);
            MazeWall52(level);
            MazeWall53(level);
            MazeWall54(level);
            MazeWall55(level);
            MazeWall56(level);
            MazeWall57(level);
            MazeWall58(level);
            MazeWall59(level);
            MazeWall60(level);
            MazeWall61(level);
            MazeWall62(level);
            MazeWall63(level);
            MazeWall64(level);
            MazeWall65(level);
            MazeWall66(level);
            MazeWall67(level);
            MazeWall68(level);
            MazeWall69(level);
            MazeWall70(level);
            MazeWall71(level);
            MazeWall72(level);
            MazeWall73(level);
            MazeWall74(level);
            MazeWall75(level);
            MazeWall76(level);
            MazeWall77(level);
            MazeWall78(level);
            MazeWall79(level);
            MazeWall80(level);
            MazeWall81(level);
            MazeWall82(level);
            MazeWall83(level);
            MazeWall84(level);
        }
        private void InitializeCollidableTriangleMeshes(Scene level)
        {
            /*
            //re - use the code on the gfx card, if we want to draw multiple objects using Clone
              var shader = new BasicShader(Application.Content, false, true);

            //create the teapot
            var complexModel = new GameObject("teapot", GameObjectType.Environment, true);
            complexModel.Transform.SetTranslation(5, 0, 0);
            complexModel.Transform.SetScale(0.1f, 0.1f, 0.1f);
            complexModel.Transform.SetRotation(0, 45, 0);
            complexModel.AddComponent(new ModelRenderer(
                modelDictionary["teapot"],
                new BasicMaterial("teapot_material", shader,
                Color.White, 1, textureDictionary["mona lisa"])));

            //add Collision Surface(s)
            collider = new Collider();
            complexModel.AddComponent(collider);
            collider.AddPrimitive(
                CollisionUtility.GetTriangleMesh(modelDictionary["teapot"],
                complexModel.Transform.LocalTranslation,
                complexModel.Transform.LocalRotation,
                complexModel.Transform.LocalScale),
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            collider.Enable(true, 1);

            //add To Scene Manager
            level.Add(complexModel);
            */
        }
        /*
        private void InitializeCollidableModels(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere", GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 5; i++)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";
                clone.Transform.SetTranslation(-5 + i / 10f, 5 + 4 * i, 0);
                clone.AddComponent(new ModelRenderer(
                    modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["checkerboard"])));

                //add Collision Surface(s)
                collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }
        */
        private void InitializeCollidableGround(Scene level, float worldScale)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the vertices and indices of the model
            var mesh = new CubeMesh();

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            //create the ground
            var ground = new GameObject("ground", GameObjectType.Ground, true);
            ground.Transform.SetTranslation(0, -1, 0);
            ground.Transform.SetScale(worldScale, 2, worldScale);
            ground.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1, textureDictionary["grass"])));

            //add Collision Surface(s)
            collider = new Collider();
            ground.AddComponent(collider);
            collider.AddPrimitive(new Box(
                    ground.Transform.LocalTranslation,
                    ground.Transform.LocalRotation,
                    ground.Transform.LocalScale),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
            collider.Enable(true, 1);

            //add To Scene Manager
            level.Add(ground);
        }

        #region Maze Walls
        #region Horizontal Walls
        private void MazeWall1(Scene level)
        {
                var shader = new BasicShader(Application.Content, false, true);
                var mesh = new CubeMesh();
                var cube = new GameObject("cube", GameObjectType.Interactable, false);
                GameObject clone = null;
                for (int i = 1; i < 2; i += 1)
                {
                    clone = cube.Clone() as GameObject;
                    clone.Transform.SetRotation(0, 90, 0);
                    clone.Transform.SetScale(5, 10, 15);
                    clone.Name = $"cube - {i}";
                    clone.Transform.Translate(20, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                    clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                    collider = new MyPlayerCollider();
                    //collider = new Collider(false, false);
                    clone.AddComponent(collider);
                    collider.AddPrimitive(new Box(
                        clone.Transform.LocalTranslation,
                        clone.Transform.LocalRotation,
                        clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                        new MaterialProperties(0f, 0f, 0f));
                    collider.Enable(true, 1);
                    //add To Scene Manager
                    level.Add(clone);
                }
        }
        private void MazeWall2(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall3(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall4(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall5(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall6(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall7(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall8(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall9(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall10(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall11(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall12(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall13(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall14(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(40, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall15(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall16(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall17(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall18(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall19(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall20(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall21(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(60, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall22(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall23(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall24(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall25(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall26(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall27(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall28(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(80, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall29(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall30(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall31(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall32(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall33(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall34(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall35(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(100, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall36(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall37(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall38(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall39(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall40(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall41(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall42(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(120, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall43(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall44(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall45(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall46(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall47(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall48(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall49(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall50(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void MazeWall51(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, 30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall52(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, 50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall53(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, 70); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall54(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall55(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -30); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall56(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -50); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }



        #endregion

        #region Vertical Walls

        private void MazeWall57(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(30, 2, 0); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall58(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(30, 2, -20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall59(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(30, 2, 40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall60(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(50, 2, 40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall61(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(50, 2, 60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall62(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(70, 2, 20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall63(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(110, 2, 20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall64(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(110, 2, 60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall65(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(90, 2, 60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall66(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(130, 2, 40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall67(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(150, 2, 80); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall68(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 80); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall69(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall70(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 80); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall71(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(70, 2, 0); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall72(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(110, 2, 0); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall73(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(70, 2, -20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall74(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(50, 2, -60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall75(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(90, 2, -60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall76(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(130, 2, -60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall77(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(110, 2, -40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall78(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(130, 2, -20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall79(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(130, 2, -40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall80(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall81(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall82(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, 20); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall83(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(160, 2, -40); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall84(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, -60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void MazeWall85(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(180, 0, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(140, 2, -60); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }























        #endregion


        #endregion

        #region soundboxes

        private void announcement(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Consumable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(1, 1, 1);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(20, 2, 15); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("yellow", shader, Color.White, 0.1f, textureDictionary["yellow"])));
                clone.AddComponent(new PickupBehaviour("audio 1", 15, "announcement"));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(false, 1);

                object[] parameters3 = { "announcement" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters3));

                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void testaudio(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Consumable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(1, 1, 1);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(30, 2, 25); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("yellow", shader, Color.White, 0.1f, textureDictionary["yellow"])));
                clone.AddComponent(new PickupBehaviour("audio 2", 10, "testaudio"));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(false, 1);

                object[] parameters1 = { "testaudio" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters1));

                //add To Scene Manager
                level.Add(clone);
            }
        }

        #endregion

        #region CubeWalls
        private void CubeWall1(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 0, 0);
                clone.Transform.SetScale(5, 10, 150);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(10, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        private void CubeWall2(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 150);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(85, 2, -63); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void CubeWall3(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 0, 0);
                clone.Transform.SetScale(5, 10, 150);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(170, 2, 10); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void CubeWall4(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 150);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(85, 2, 85); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void CubeWall5(Scene level)
        {
            var shader = new BasicShader(Application.Content, false, true);
            var mesh = new CubeMesh();
            var cube = new GameObject("cube", GameObjectType.Interactable, false);
            GameObject clone = null;
            for (int i = 1; i < 2; i += 1)
            {
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 90, 0);
                clone.Transform.SetScale(5, 10, 15);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(167.5f, 2, 85); //clone.Transform.Translate(10, 4f * (1 + i), 10);
                clone.AddComponent(new MeshRenderer(mesh, new BasicMaterial("grass_material", shader, Color.White, 1f, textureDictionary["grass"])));

                collider = new MyPlayerCollider();
                //collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0f, 0f, 0f));
                collider.Enable(true, 1);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        #endregion

        #region sphere
        private void InitializeSphere(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere", GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 1; i+= 1)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";
                clone.Transform.SetTranslation(165 + i / -500f, 5 + 4 * i, -63);
                clone.AddComponent(new ModelRenderer(
                    modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["blue"])));

                //add Collision Surface(s)
                collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void InitializeSphere2(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere", GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 1; i += 1)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";
                clone.Transform.SetTranslation(90 + i / -500f, 5 + 4 * i, -5);
                clone.AddComponent(new ModelRenderer(
                    modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["blue"])));

                //add Collision Surface(s)
                collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }

        private void InitializeSphere3(Scene level)
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);

            //create the sphere
            var sphereArchetype = new GameObject("sphere", GameObjectType.Interactable, true);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 1; i += 1)
            {
                clone = sphereArchetype.Clone() as GameObject;
                clone.Name = $"sphere - {i}";
                clone.Transform.SetTranslation(35 + i / -500f, 5 + 4 * i, -35);
                clone.AddComponent(new ModelRenderer(
                    modelDictionary["sphere"],
                    new BasicMaterial("sphere_material",
                    shader, Color.White, 1, textureDictionary["blue"])));

                //add Collision Surface(s)
                collider = new Collider(false, false);
                clone.AddComponent(collider);
                collider.AddPrimitive(new JigLibX.Geometry.Sphere(
                   sphereArchetype.Transform.LocalTranslation, 1),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 1);

                //add To Scene Manager
                level.Add(clone);
            }
        }

        #endregion

        private void InitializeCollidableCubes(Scene level)//delete once done with cubes//
        {
            #region Reusable - You can copy and re-use this code elsewhere, if required

            //re-use the code on the gfx card, if we want to draw multiple objects using Clone
            var shader = new BasicShader(Application.Content, false, true);
            //re-use the mesh
            var mesh = new CubeMesh();
            //clone the cube
            var cube = new GameObject("cube", GameObjectType.Consumable, false);

            #endregion Reusable - You can copy and re-use this code elsewhere, if required

            GameObject clone = null;

            for (int i = 0; i < 5; i++)
            {
                //clone the archetypal cube
                clone = cube.Clone() as GameObject;
                clone.Transform.SetRotation(0, 45, 0);
                clone.Transform.SetScale(1, 1, 1);
                clone.Name = $"cube - {i}";
                clone.Transform.Translate(5, 4f * (1 + i), 0);
                clone.AddComponent(new MeshRenderer(mesh,
                    new BasicMaterial("cube_material", shader,
                    Color.White, 1f, textureDictionary["crate1"])));


                //add Collision Surface(s)
                collider = new MyPlayerCollider();

                clone.AddComponent(collider);
                collider.AddPrimitive(new Box(
                    clone.Transform.LocalTranslation,
                    clone.Transform.LocalRotation,
                    clone.Transform.LocalScale * 1.01f), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
                collider.Enable(false, 10);
                //add To Scene Manager
                level.Add(clone);
            }
        }
        #endregion Student/Group Specific Code

        /******************************* Demo (Remove For Release) *******************************/
        #region Demo Code

#if DEMO

        public delegate void MyDelegate(string s, bool b);

        public List<MyDelegate> delList = new List<MyDelegate>();

        public void DoSomething(string msg, bool enableIt)
        {
        }

        private void InitializeEditorHelpers()
        {
            //a game object to record camera positions to an XML file for use in a curve later
            var curveRecorder = new GameObject("curve recorder", GameObjectType.Editor);
            curveRecorder.AddComponent(new GDLibrary.Editor.CurveRecorderController());
            activeScene.Add(curveRecorder);
        }

        private void RunDemos()
        {
            // CurveDemo();
            // SaveLoadDemo();

            EventSenderDemo();
        }

        private void EventSenderDemo()
        {
            var myDel = new MyDelegate(DoSomething);
            myDel("sdfsdfdf", true);
            delList.Add(DoSomething);
        }

        private void CurveDemo()
        {
            //var curve1D = new GDLibrary.Parameters.Curve1D(CurveLoopType.Cycle);
            //curve1D.Add(0, 0);
            //curve1D.Add(10, 1000);
            //curve1D.Add(20, 2000);
            //curve1D.Add(40, 4000);
            //curve1D.Add(60, 6000);
            //var value = curve1D.Evaluate(500, 2);
        }

        private void SaveLoadDemo()
        {
        #region Serialization Single Object Demo

            var demoSaveLoad = new DemoSaveLoad(new Vector3(1, 2, 3), new Vector3(45, 90, -180), new Vector3(1.5f, 0.1f, 20.25f));
            GDLibrary.Utilities.SerializationUtility.Save("DemoSingle.xml", demoSaveLoad);
            var readSingle = GDLibrary.Utilities.SerializationUtility.Load("DemoSingle.xml",
                typeof(DemoSaveLoad)) as DemoSaveLoad;

        #endregion Serialization Single Object Demo

        #region Serialization List Objects Demo

            List<DemoSaveLoad> listDemos = new List<DemoSaveLoad>();
            listDemos.Add(new DemoSaveLoad(new Vector3(1, 2, 3), new Vector3(45, 90, -180), new Vector3(1.5f, 0.1f, 20.25f)));
            listDemos.Add(new DemoSaveLoad(new Vector3(10, 20, 30), new Vector3(4, 9, -18), new Vector3(15f, 1f, 202.5f)));
            listDemos.Add(new DemoSaveLoad(new Vector3(100, 200, 300), new Vector3(145, 290, -80), new Vector3(6.5f, 1.1f, 8.05f)));

            GDLibrary.Utilities.SerializationUtility.Save("ListDemo.xml", listDemos);
            var readList = GDLibrary.Utilities.SerializationUtility.Load("ListDemo.xml",
                typeof(List<DemoSaveLoad>)) as List<DemoSaveLoad>;

        #endregion Serialization List Objects Demo
        }

#endif

        #endregion Demo Code
    }
}