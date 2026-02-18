namespace drawedOut
{
    partial class MainMenu
    {
        #region MainMenu compoments code

        private static GameButton 
            _playBtn,
            _quitBtn,
            _settingsBtn,
            _backBtn;

        private void CreateMenuButtons()
        {
            // main menu
            _quitBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.75f,
                    relWidth:0.1f,
                    relHeight: 0.06f,
                    clickEvent: QuitGame,
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
                    clickEvent: ShowLevelMenu,
                    fontScale: 2f,
                    txt: "Play");
            _backBtn = new GameButton(
                    xCenterPos: 0.5f,
                    yCenterPos: 0.9f,
                    relWidth: 0.1f,
                    relHeight: 0.05f,
                    clickEvent: ShowMainMenu,
                    fontSize: 20,
                    txt: "Back to title");
        }
        # endregion



        # region Settings Menu region

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
            _abilityThreeRebindBtn;

        private string BoolToString(bool input) => (input) ? "On" : "Off"; 

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
            _settingsStringsYPos.Add($"FPS (current: {Global.GameTickFreq})", _24FpsBtn.Y);
            _settingsStringsYPos.Add($"Game resolution \n(current: {Global.LevelResolution.ToString()})", _720pBtn.Y);
            Invalidate();
        }

        private void CreateSettingsBtns()
        {
            // settings menu
            _backgroundBtn = new GameButton(
                    xCenterPos: 0.55f,
                    yCenterPos: 0.18f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: BgBtnClick,
                    txt: BoolToString(Global.ShowBG));
            CreateFPSBtns(0.54f, 0.33f);
            CreateResBtns(0.55f, 0.40f);
            _timeBtn = new GameButton(
                    xCenterPos: 0.55f,
                    yCenterPos: 0.25f,
                    relWidth: 0.06f,
                    relHeight: 0.05f,
                    clickEvent: TimeBtnClick,
                    txt: BoolToString(Global.ShowTime));

            CreateBindBtns(0.4f, 0.6f);
            CreateBindStrings();
        }

        private void CreateFPSBtns(float xLevel, float yLevel)
        {
            int fontSize = 20;
            float spacing = 0.05f;
            float btnWidth = 0.03f;
            float btnHeight = 0.035f;
            _24FpsBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=> FpsBtnClick(24),
                    txt: "24",
                    fontSize: fontSize);
            _30FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=> FpsBtnClick(30),
                    txt: "30",
                    fontSize: fontSize);
            _60FpsBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=> FpsBtnClick(60),
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
            int fontSize = 20;
            float spacing = 0.07f;
            float btnWidth = 0.05f;
            float btnHeight = 0.035f;
            _720pBtn = new GameButton(
                    xCenterPos: xLevel,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>resBtnClick(Global.Resolutions.p720),
                    txt: $"720p",
                    fontSize: fontSize);
            _1080pBtn = new GameButton(
                    xCenterPos: xLevel+spacing,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>resBtnClick(Global.Resolutions.p1080),
                    txt: "1080p",
                    fontSize: fontSize);
            _1440pBtn = new GameButton(
                    xCenterPos: xLevel+spacing*2,
                    yCenterPos: yLevel,
                    relWidth: btnWidth,
                    relHeight: btnHeight,
                    clickEvent: ()=>resBtnClick(Global.Resolutions.p1440),
                    txt: "1440p",
                    fontSize: fontSize);
        }

        private void CreateBindBtns(float xOrigin, float yPos)
        {
            int fontSize=19;
            float xOffset = 0.3f;
            float yOffset = 0.08f;
            float width = 0.12f;
            float height = 0.035f;

            _leftRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{ 
                    _rebindAction=Keybinds.Actions.MoveLeft; 
                    Invalidate();
                    },
                    fontSize: fontSize);
            _rightRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos+yOffset,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{
                    _rebindAction=Keybinds.Actions.MoveRight;
                    Invalidate();
                    },
                    fontSize: fontSize);
            _jumpRebindBtn = new GameButton(
                    xCenterPos: xOrigin,
                    yCenterPos: yPos+yOffset*2,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{
                    _rebindAction=Keybinds.Actions.Jump;
                    Invalidate();
                    },
                    fontSize: fontSize);

            _abilityOneRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{
                    _rebindAction=Keybinds.Actions.Special1;
                    Invalidate();
                    },
                    fontSize: fontSize);
            _abilityTwoRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos+yOffset,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{
                    _rebindAction=Keybinds.Actions.Special2;
                    Invalidate();
                    },
                    fontSize: fontSize);
            _abilityThreeRebindBtn = new GameButton(
                    xCenterPos: xOrigin+xOffset,
                    yCenterPos: yPos+yOffset*2,
                    relWidth: width,
                    relHeight: height,
                    clickEvent: ()=>{
                    _rebindAction=Keybinds.Actions.Special3;
                    Invalidate();
                    },
                    fontSize: fontSize);
        }

        private void UpdateKeyBtnStrings()
        {
            if (_rebindAction is null) 
            {
                _jumpRebindBtn.BtnTxt = "Click to rebind";
                _leftRebindBtn.BtnTxt = "Click to rebind";
                _rightRebindBtn.BtnTxt = "Click to rebind";
                _abilityOneRebindBtn.BtnTxt = "Click to rebind";
                _abilityTwoRebindBtn.BtnTxt = "Click to rebind";
                _abilityThreeRebindBtn.BtnTxt = "Click to rebind";
                return;
            }
            switch (_rebindAction.Value)
            {
                case (Keybinds.Actions.MoveLeft):
                    _leftRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.MoveRight):
                    _rightRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Jump):
                    _jumpRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special1):
                    _abilityOneRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special2):
                    _abilityTwoRebindBtn.BtnTxt="Enter new key";
                    break;
                case (Keybinds.Actions.Special3):
                    _abilityThreeRebindBtn.BtnTxt="Enter new key";
                    break;
            }
        }


        private void CreateBindStrings()
        {
            _bindingStringsPos.Clear();
            float xOffset = 0.15f*Width;

            int curX = _jumpRebindBtn.X - (int)xOffset;
            _bindingStringsPos.Add(
                    $"Move Left: {Keybinds.ActionBindings[Keybinds.Actions.MoveLeft]}", 
                    new Point(curX, _leftRebindBtn.Y));
            _bindingStringsPos.Add(
                    $"Move Right: {Keybinds.ActionBindings[Keybinds.Actions.MoveRight]}", 
                    new Point(curX, _rightRebindBtn.Y));
            _bindingStringsPos.Add(
                    $"Jump: {Keybinds.ActionBindings[Keybinds.Actions.Jump]}", 
                    new Point(curX, _jumpRebindBtn.Y));

            curX = _abilityOneRebindBtn.X - (int) xOffset;
            _bindingStringsPos.Add(
                    $"Ability One: {Keybinds.ActionBindings[Keybinds.Actions.Special1]}", 
                    new Point(curX, _abilityOneRebindBtn.Y));
            _bindingStringsPos.Add(
                    $"Ability Two: {Keybinds.ActionBindings[Keybinds.Actions.Special2]}",
                    new Point(curX, _abilityTwoRebindBtn.Y));
            _bindingStringsPos.Add(
                    $"Ability Three: {Keybinds.ActionBindings[Keybinds.Actions.Special3]}", 
                    new Point(curX, _abilityThreeRebindBtn.Y));
        }
        #endregion

        # region level select menu

        private const float 
            _COMPONENT_X_ORIGIN = 0.45f,
            _COMPONENT_Y_ORIGIN = 0.25f,
            _BTN_WIDTH = 0.2f,
            _BTN_HEIGHT = 0.08f,
            _COMPONENT_OFFSET_Y = 0.16f;
        private const int _TIME_OFFSET_Y = 8; 

        private static int _rankSize;
        private static float _timeOffsetY;
        private static Ranks?
            _tutorialRank,
            _level1Rank,
            _level2Rank;
        private static Bitmap?
            _tutorialRankDisplay,
            _level1RankDisplay,
            _level2RankDisplay;
        private static int[] _rankArray = new int[3];

        private GameButton
            _tutorialBtn,
            _level1Btn,
            _level2Btn;

        private void CreateLevelButtons()
        {
            _tutorialBtn = new GameButton(
                    xCenterPos: _COMPONENT_X_ORIGIN,
                    yCenterPos: _COMPONENT_Y_ORIGIN,
                    relWidth: _BTN_WIDTH,
                    relHeight: _BTN_HEIGHT,
                    clickEvent: LoadTutorial,
                    txt: "Tutorial",
                    fontScale: 1.5f);
            _level1Btn = new GameButton(
                    xCenterPos: _COMPONENT_X_ORIGIN,
                    yCenterPos: _COMPONENT_Y_ORIGIN+_COMPONENT_OFFSET_Y,
                    relWidth: _BTN_WIDTH,
                    relHeight: _BTN_HEIGHT,
                    clickEvent: LoadLevel1,
                    txt: "Level 1",
                    fontScale: 1.5f);
            _level2Btn = new GameButton(
                    xCenterPos: _COMPONENT_X_ORIGIN,
                    yCenterPos: _COMPONENT_Y_ORIGIN+_COMPONENT_OFFSET_Y*2,
                    relWidth: _BTN_WIDTH,
                    relHeight: _BTN_HEIGHT,
                    clickEvent: LoadLevel2,
                    txt: "Level 2",
                    fontScale: 1.5f);
            
            _timeOffsetY = _TIME_OFFSET_Y*Global.BaseScale;
            _rankSize = (int)Math.Ceiling(_level1Btn.Rect.Height + Global.DefaultFont.Size*1.5 +_timeOffsetY);
        }
        #endregion
    }
}
