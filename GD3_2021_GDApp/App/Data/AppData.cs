#define HI_RES

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDApp
{
    public static class AppData
    {
        public static readonly string CAMERA_FIRSTPERSON_NONCOLLIDABLE_NAME = "main noncollidable";
        public static readonly string CAMERA_FIRSTPERSON_COLLIDABLE_NAME = "main collidable";
        public static readonly string CAMERA_CURVE_NONCOLLIDABLE_NAME = "curve noncollidable";

        #region Game Level Constants

        public const string GAME_TITLE_NAME = "My Game Title Goes Here";

#if HI_RES
        public const int GAME_RESOLUTION_WIDTH = 1920;
        public const int GAME_RESOLUTION_HEIGHT = 1080;

#else
        public const int GAME_RESOLUTION_WIDTH = 640;
        public const int GAME_RESOLUTION_HEIGHT = 480;
#endif

        #endregion Game Level Constants

        #region UI & Menu Constants

        public const string UI_SCENE_MAIN_NAME = "main ui";
        public const string MENU_MAIN_NAME = "main menu";
        public const string MENU_CONTROLS_NAME = "controls menu";

        public const string MENU_PLAY_BTN_NAME = "Play";
        public const string MENU_CONTROLS_BTN_NAME = "Controls";
        public const string MENU_BACK_BTN_NAME = "Back";
        public const string MENU_EXIT_BTN_NAME = "Exit";

        //set button positions dynamically from resolution
        public static readonly Vector2 MENU_PLAY_BTN_POSITION
            = new Vector2(GAME_RESOLUTION_WIDTH / 2, GAME_RESOLUTION_HEIGHT / 2.0f);

        public static readonly Vector2 MENU_CONTROLS_BTN_POSITION
            = new Vector2(GAME_RESOLUTION_WIDTH / 2, GAME_RESOLUTION_HEIGHT / 2.0f + 40);

        public static readonly Vector2 MENU_EXIT_BTN_POSITION
            = new Vector2(GAME_RESOLUTION_WIDTH / 2, GAME_RESOLUTION_HEIGHT / 2.0f + 80);

        #endregion UI & Menu Constants

        #region Input Key Mappings

        public static readonly Keys[] KEYS_ONE = { Keys.W, Keys.S, Keys.A, Keys.D };
        public static readonly Keys[] KEYS_TWO = { Keys.U, Keys.J, Keys.H, Keys.K };

        #endregion Input Key Mappings

        #region Movement Constants

        public const float PLAYER_MOVE_SPEED = 0.1f;
        private const float PLAYER_STRAFE_SPEED_MULTIPLIER = 0.75f;
        public const float PLAYER_STRAFE_SPEED = PLAYER_STRAFE_SPEED_MULTIPLIER * PLAYER_MOVE_SPEED;
        public const float PLAYER_ROTATE_SPEED = 0.01f;

        #endregion Movement Constants

        //unique IDs for controllers, behaviours, cameras, scenes, uiscenes
    }
}