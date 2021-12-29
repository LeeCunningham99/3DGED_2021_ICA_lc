using GDLibrary;
using GDLibrary.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GDApp
{
    /// <summary>
    /// This component will check for win/lose logic in the game
    /// </summary>
    public class MyStateManager : PausableGameComponent
    {
        public List<string> inventory;
        private float timeSinceLastStateCheck;

        public MyStateManager(Game game)
            : base(game)
        {
            inventory = new List<string>();
        }

        protected override void SubscribeToEvents()
        {
            //add more events here...
            EventDispatcher.Subscribe(EventCategoryType.Player,
                HandleEvent);

            EventDispatcher.Subscribe(EventCategoryType.Inventory,
                HandleInventory);

            EventDispatcher.Subscribe(EventCategoryType.GameState,
              HandleGameState);

            //dont forget to call base otherwise no play/pause support
            base.SubscribeToEvents();
        }

        private void HandleGameState(EventData eventData)
        {
            //the code below is a demo of responding to a win or lose game event that shows a UI scene and then shows main scene
            if (eventData.EventActionType == EventActionType.OnWin)
            {
                //Application.UISceneManager.SetActiveScene(AppData.WinScene);
                //count time, when X ms have elapsed
                //https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-6.0
                //var  aTimer = new System.Timers.Timer(2000);
                //aTimer.Elapsed += ShowMainMenu;
                //aTimer.AutoReset = false;
                //aTimer.Enabled = true;
                //Application.UISceneManager.SetActiveScene(AppData.MainMenuScene);
            }
            else if (eventData.EventActionType == EventActionType.OnWin)
            {
                //Application.UISceneManager.SetActiveScene(AppData.LoseScene);
                ////count time, when X ms have elapsed
                //Application.UISceneManager.SetActiveScene(AppData.MainMenuScene);
            }
        }

        private void HandleInventory(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnAddInventory)
            {
                //add something to list
                string newThing = eventData.Parameters[0] as string; //"sword"
                inventory.Add(newThing);
            }
            else if (eventData.EventActionType == EventActionType.OnRemoveInventory)
            {
                //remove something from list
            }
        }

        protected override void HandleEvent(EventData eventData)
        {
            //add more event handlers here...
            if (eventData.EventActionType == EventActionType.OnPickup)
            {
                var objectName = eventData.Parameters[0] as string;
            }
            //dont forget to call base otherwise no play/pause support
            base.HandleEvent(eventData);
        }

        public override void Update(GameTime gameTime)
        {
            timeSinceLastStateCheck += Time.Instance.DeltaTimeMs;

            if (timeSinceLastStateCheck >= 500)
            {
                timeSinceLastStateCheck -= 500;

                //do we need to periodically check something like win/lose state?
                if (inventory.Contains("sword"))
                {
                }

                if (inventory.Count == 5)
                {
                    //play sound
                    //change camera Application.SceneManager.ActiveScene.SetMainCamera("dfdf");
                    //event for ui element?
                    //event for showing main menu
                }
            }

            base.Update(gameTime);
        }
    }
}