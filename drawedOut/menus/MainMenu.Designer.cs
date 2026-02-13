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
            _fpsTxt,
            _resTxt,

            _jumpTxt,
            _leftTxt,
            _rightTxt,
            _abilityOneTxt,
            _abilityTwoTxt,
            _abilityThreeTxt;

        private static Dictionary<String, int> _settingsStringsYPos = 
            new Dictionary<String, int>();
        private static Dictionary<String, Point> _bindingStringsPos = 
            new Dictionary<String, Point>();

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
            _settingsStringsYPos.Clear();
            _settingsStringsYPos.Add("Show lined background", _backgroundBtn.Y);
            _settingsStringsYPos.Add("Show time in level", _timeBtn.Y);
            _settingsStringsYPos.Add("_fpsTxt", _24FpsBtn.Y);
            _settingsStringsYPos.Add("Game display resolution", _720pBtn.Y);
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
            
            CreateBindBtns(0.4f, 0.55f);
            CreateBindStrings();

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

        private void CreateBindBtns(float xOrigin, float yPos)
        {
            float xOffset = 0.3f;
            float yOffset = 0.08f;
            float width = 0.12f;
            float height = 0.035f;

            _leftRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");
            _rightRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos+yOffset,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");
            _jumpRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos+yOffset*2,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");

            _abilityOneRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");
            _abilityTwoRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos+yOffset,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");
            _abilityThreeRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos+yOffset*2,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{},
                    txt: "Click to rebind");
        }

        private void CreateBindStrings()
        {
            _bindingStringsPos.Clear();
            float xOffset = 0.15f*Width;

            int curX = _jumpRebindBtn.X - (int)xOffset;
            _bindingStringsPos.Add("Move Left: ", new Point(curX, _leftRebindBtn.Y));
            _bindingStringsPos.Add("Move Right: ", new Point(curX, _rightRebindBtn.Y));
            _bindingStringsPos.Add("Jump: ", new Point(curX, _jumpRebindBtn.Y));

            curX = _abilityOneRebindBtn.X - (int) xOffset;
            _bindingStringsPos.Add("Ability One:", new Point(curX, _abilityOneRebindBtn.Y));
            _bindingStringsPos.Add("Ability Two:", new Point(curX, _abilityTwoRebindBtn.Y));
            _bindingStringsPos.Add("Ability Three:", new Point(curX, _abilityThreeRebindBtn.Y));
        }

        #endregion

    }
}
