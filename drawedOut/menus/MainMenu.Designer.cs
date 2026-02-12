namespace drawedOut
{
    partial class MainMenu
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "MainMenu";
            Text = "MainMenu";
            Load += MainMenu_Load;
            FormClosing += MainMenu_Quit;
            Paint += MainMenu_Paint;
            MouseDown += MainMenu_MouseDown;
            KeyDown += MainMenu_KeyDown;
            ResumeLayout(false);
        }

        #endregion

        #region MainMenu compoments code

        private static GameButton 
            _playBtn,
            _quitBtn,
            _settingsBtn;

        // Settings menu components
        private static string 
            _backgroundTxt,
            _fpsTxt,
            _resTxt,
            _timeTxt,

            _keysHeading,
            _jumpTxt,
            _leftTxt,
            _rightTxt,
            _abilityOneTxt,
            _abilityTwoTxt,
            _abilityThreeTxt;

        private static GameButton 
            _backgroundBtn,

            _24FpsBtn,
            _30FpsBtn,
            _60FpsBtn,
            _120FpsBtn,

            _720pBtn,
            _1080pBtn,
            _1440pBtn,

            _timeBtn,

            _jumpRebindBtn,
            _leftRebindBtn,
            _rightRebindBtn,
            _abilityOneRebindBtn,
            _abilityTwoRebindBtn,
            _abilityThreeRebindBtn,
            
            _settingsBackBtn;
        

        private string BoolToString(bool input) => (input) ? "On" : "Off"; 

        private void CreateMenuButtons()
        {
            // main menu
            _quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.75f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: ()=>TryInvoke(QuitGame),
                    fontScale: 1.2f,
                    txt: "Quit");
            _settingsBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.55f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: ShowSettingsMenu,
                    fontScale: 2f, 
                    txt: "Settings");
            _playBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.4f,
                    relWidth: 0.2f,
                    relHeight: 0.1f,
                    clickEvent: ()=>TryInvoke(OpenLevelMenu),
                    fontScale: 2f,
                    txt: "Play");
        }

        private void InitSettings()
        {
            CreateSettingsBtns();
            CreateSettingsStrings();
        }

        private void CreateSettingsStrings()
        {
            _backgroundTxt = "Show lined background";
            _fpsTxt = "Game tick rate";
            _resTxt = "Game display resolution";
            _timeTxt = "Show time in level";
        }

        private void CreateSettingsBtns()
        {
            // settings menu
            _backgroundBtn = new GameButton(
                    xCenterPos: 0.55f,
                    yCenterPos: 0.18f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: ()=>{
                        Global.ShowBG=!Global.ShowBG;
                        _backgroundBtn.BtnTxt = BoolToString(Global.ShowBG);
                    },
                    txt: BoolToString(Global.ShowBG));
            CreateFPSBtns(0.54f, 0.33f);
            CreateResBtns(0.55f, 0.40f);
            _timeBtn = new GameButton(
                    xCenterPos: 0.55f,
                    yCenterPos: 0.25f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: ()=> {
                        Global.ShowTime=!Global.ShowTime;
                        _timeBtn.BtnTxt = BoolToString(Global.ShowTime);
                    },
                    txt: BoolToString(Global.ShowTime));
            //
            // _leftRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.54f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: LeftRebindClick,
            //         txt: "Click to rebind");
            // _rightRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.56f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: RightRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.4f,
            //         yCenterPos: 0.58f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: JumpRebindClick,
            //         txt: "Click to rebind");
            // _abilityOneRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.54f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityOneRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.56f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityTwoRebindClick,
            //         txt: "Click to rebind");
            // _jumpRebindBtn = new GameButton(
            //         xCenterPos: 0.8f,
            //         yCenterPos: 0.58f,
            //         relWidth: 0.15f,
            //         relHeight: 0.05f,
            //         clickEvent: AbilityThreeRebindClick,
            //         txt: "Click to rebind");
            //
            _settingsBackBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.9f,
                    relWidth: 0.1f,
                    relHeight: 0.05f,
                    clickEvent: ShowMainMenu,
                    txt: "Back to menu");
        }

        private void CreateFPSBtns(float xLevel, float yLevel)
        {
            int fontSize = 15;
            float spacing = 0.05f;
            float btnWidth = 0.03f;
            float btnHeight = 0.035f;
            _24FpsBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(24),
                    txt: "24",
                    fontSize: fontSize);
            _30FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(30),
                    txt: "30",
                    fontSize: fontSize);
            _60FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(60),
                    txt: "60",
                    fontSize: fontSize);
            _120FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing*3,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>FpsBtnClick(120),
                    txt: "120",
                    fontSize: fontSize);
        }

        private void CreateResBtns(float xLevel, float yLevel)
        {
            int fontSize = 15;
            float spacing = 0.07f;
            float btnWidth = 0.05f;
            float btnHeight = 0.035f;
            _720pBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{
                        Global.LevelResolution = Global.Resolutions.p720;
                        UpdateSize();
                    },
                    txt: "720p",
                    fontSize: fontSize);
            _1080pBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{
                        Global.LevelResolution = Global.Resolutions.p1080;
                        UpdateSize();
                    },
                    txt: "1080p",
                    fontSize: fontSize);
            _1440pBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>{
                        Global.LevelResolution = Global.Resolutions.p1440;
                        UpdateSize();
                    },
                    txt: "1440p",
                    fontSize: fontSize);
        }

        private void ShowMainMenu()
        {
            _curMenuState = MenuState.Start;

            GameButton.HideAll();
            _playBtn.Show();
            _settingsBtn.Show();
            _quitBtn.Show();
        }

        private void ShowSettingsMenu()
        {
            _curMenuState = MenuState.Settings;

            GameButton.HideAll();
            _backgroundBtn.Show();

            _24FpsBtn.Show();
            _30FpsBtn.Show();
            _60FpsBtn.Show();
            _120FpsBtn.Show();

            _720pBtn.Show();
            _1080pBtn.Show();
            _1440pBtn.Show();

            _timeBtn.Show();

            _settingsBackBtn.Show();

            TryInvoke(Invalidate);
        }

        #endregion
    }
}
