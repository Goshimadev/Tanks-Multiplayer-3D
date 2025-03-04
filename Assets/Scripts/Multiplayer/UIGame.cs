/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// UI script for all elements, team events and user interactions in the game scene.
    /// </summary>
    public class UIGame : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Joystick components controlling player movement and actions on mobile devices.
        /// </summary>
        public FixedJoystick controls;
        public Joybutton controlsFire;
        public BackButton controlsBack;

        /// <summary>
        /// UI sliders displaying team fill for each team using absolute values.
        /// </summary>
        public Slider[] teamSize;

        /// <summary>
        /// UI texts displaying kill scores for each team.
        /// </summary>
        public Text[] teamScore;

        /// <summary>
        /// UI texts displaying kill scores for this local player.
        /// [0] = Kill Count, [1] = Death Count
        /// </summary>
        public Text[] killCounter;

        /// <summary>
        /// UI text for indicating player death and who killed this player.
        /// </summary>
        public Text deathText;

        /// <summary>
        /// UI text displaying the time in seconds left until player respawn.
        /// </summary>
        public Text spawnDelayText;

        /// <summary>
        /// UI text for indicating game end and which team has won the round.
        /// </summary>
        public Text gameOverText;

        /// <summary>
        /// UI window gameobject activated on game end, offering sharing and restart buttons.
        /// </summary>
        public GameObject gameOverMenu;

        public GameObject messenger;

        public Text textMsg;

        public InputField inputMsg;

        public Button sendMsgBtn;

        private Player[] players;

        private MultGameManager gameManager;


        //initialize variables
        void Start()
        {
            //on non-mobile devices hide joystick controls, except in editor
#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_WEBGL)
                ToggleControls(false);
#endif
            deathText.gameObject.SetActive(false);
            spawnDelayText.gameObject.SetActive(false);

            gameManager = FindObjectOfType<MultGameManager>();

            //on mobile devices enable additional aiming indicator
            /*#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
            if (aimIndicator != null)
            {
                Transform indicator = Instantiate(aimIndicator).transform;
                indicator.SetParent(GameManager.GetInstance().localPlayer.shotPos);
                indicator.localPosition = new Vector3(0f, 0f, 3f);
            }
            #endif */

            

            //play background music
            AudioManager.PlayMusic(1);
        }

        public void SendMsg(string text, string owner, int team)
        {
            textMsg.text = textMsg.text + "<color=#" + ColorUtility.ToHtmlStringRGB(gameManager.teams[team].material.color) + ">" + owner + ": </color>" + text + "\n";
        }

        public string GetText()
        {
            return textMsg.text;
        }

        public void SetPlayerConnected(string name, int team)
        {
            textMsg.text = textMsg.text + "<color=#" + ColorUtility.ToHtmlStringRGB(gameManager.teams[team].material.color) + ">" + name + " connected</color>\n";

        }

        /// <summary>
        /// This method gets called whenever room properties have been changed on the network.
        /// Updating our team size and score UI display during the game.
        /// See the official Photon docs for more details.
        /// </summary>
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			OnTeamSizeChanged(PhotonNetwork.CurrentRoom.GetSize());
			OnTeamScoreChanged(PhotonNetwork.CurrentRoom.GetScore());
            //SetPlayerConnected(PlayerPrefs.GetString(PrefsKeys.playerName));
        }


        /// <summary>
        /// This is an implementation for changes to the team fill,
        /// updating the slider values (updates UI display of team fill).
        /// </summary>
        public void OnTeamSizeChanged(int[] size)
        {
            //loop over sliders values and assign it
			for(int i = 0; i < size.Length; i++)
            	teamSize[i].value = size[i];
        }


        /// <summary>
        /// This is an implementation for changes to the team score,
        /// updating the text values (updates UI display of team scores).
        /// </summary>
        public void OnTeamScoreChanged(int[] score)
        {
            //loop over texts
			for(int i = 0; i < score.Length; i++)
            {
                //detect if the score has been increased, then add fancy animation
                if(score[i] > int.Parse(teamScore[i].text))
                    teamScore[i].GetComponent<Animator>().Play("Animation");

                //assign score value to text
                teamScore[i].text = score[i].ToString();
            }
        }


        /// <summary>
        /// Enables or disables visibility of joystick controls.
        /// </summary>
        public void ToggleControls(bool state)
        {
            controls.gameObject.SetActive(state);
            controlsFire.gameObject.SetActive(state);
            controlsBack.gameObject.SetActive(state);
        }


        /// <summary>
        /// Sets death text showing who killed the player in its team color.
        /// Parameters: killer's name, killer's team
        /// </summary>
        public void SetDeathText(string playerName, MultGameManager.Team team)
        {
            //hide joystick controls while displaying death text
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(false);
#endif

            //show killer name and colorize the name converting its team color to an HTML RGB hex value for UI markup
            deathText.gameObject.SetActive(true);
            deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + playerName + "</color>";
        }
        
        
        /// <summary>
        /// Set respawn delay value displayed to the absolute time value received.
        /// The remaining time value is calculated in a coroutine by GameManager.
        /// </summary>
        public void SetSpawnDelay(float time)
        {
            spawnDelayText.gameObject.SetActive(true);
            spawnDelayText.text = Mathf.Ceil(time) + "";
        }
        
        
        /// <summary>
        /// Hides any UI components related to player death after respawn.
        /// </summary>
        public void DisableDeath()
        {
            //show joystick controls after disabling death text
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(true);
            #endif
            
            //clear text component values
            deathText.text = string.Empty;
            spawnDelayText.text = string.Empty;
            deathText.gameObject.SetActive(false);
            spawnDelayText.gameObject.SetActive(false);
        }


        /// <summary>
        /// Set game end text and display winning team in its team color.
        /// </summary>
        public void SetGameOverText(MultGameManager.Team team)
        {
            //hide joystick controls while displaying game end text
            #if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
                ToggleControls(false);
            #endif
            
            //show winning team and colorize it by converting the team color to an HTML RGB hex value for UI markup
            gameOverText.text = "TEAM <color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + team.name + "</color> WINS!";
        }


        /// <summary>
        /// Displays the game's end screen. Called by GameManager after few seconds delay.
        /// Tries to display a video ad, if not shown already.
        /// </summary>
        public void ShowGameOver()
        {       
            //hide text but enable game over window
            gameOverText.gameObject.SetActive(false);
            gameOverMenu.SetActive(true);
            
            //check whether an ad was shown during the game
            //if no ad was shown during the whole round, we request one here
            #if UNITY_ADS
            if(!UnityAdsManager.didShowAd())
                UnityAdsManager.ShowAd(true);
            #endif
        }


        /// <summary>
        /// Returns to the starting scene and immediately requests another game session.
        /// In the starting scene we have the loading screen and disconnect handling set up already,
        /// so this saves us additional work of doing the same logic twice in the game scene. The
        /// restart request is implemented in another gameobject that lives throughout scene changes.
        /// </summary>
        public void Restart()
        {
            GameObject gObj = new GameObject("RestartNow");
            gObj.AddComponent<UIRestartButton>();
            DontDestroyOnLoad(gObj);
            
            Quit();
        }


        /// <summary>
        /// Stops receiving further network updates by hard disconnecting, then load starting scene.
        /// </summary>
        public void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
            Quit();
        }


        /// <summary>
        /// Loads the starting scene. Disconnecting already happened when presenting the GameOver screen.
        /// </summary>
        public void Quit()
        {
            SceneManager.LoadScene(NetworkManagerCustom.GetInstance().offlineSceneIndex);
        }
    }
}
