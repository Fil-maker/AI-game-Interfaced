using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AI_game;

namespace AI_game_interfaced
{
    enum ClickedMenus
    {
        Field,
        Shop,
        NotClicked
    }

    enum GamePhases
    {
        Menu,
        Game,
        Pause
    }
    class BuildingObject
    {

        #region buildings
        public static BuildingObject[] BuildingLibrary = new BuildingObject[]
        {
            new BuildingObject(new LivingBuilding(70, 1),400, new[] {"sprites\\Living_3_1.png", "sprites\\Living_3_2.png" }),
            new BuildingObject(new LivingBuilding(50, 4),500, new[] {"sprites\\Living_4_1.png", "sprites\\Living_4_2.png" }),
            new BuildingObject(new LivingBuilding(30, 5),700, new[] {"sprites\\Living_5_1.png", "sprites\\Living_5_2.png" }),
            new BuildingObject(new EntartainingBuliding(4, new Tolerance(1, 1.2, .1, .5, .5)), 300,
                new[] {"sprites\\Museum1.png", "sprites\\Museum2.png" }),
            new BuildingObject(new EntartainingBuliding(3, new Tolerance(1, .5, .5, 1, .9)), 500,
                new[] {"sprites\\Police1.png", "sprites\\Police2.png" }),
            new BuildingObject(new EntartainingBuliding(5, new Tolerance(.5, .5, .5, 1.2, 1.5)),500,
                new[] {"sprites\\Resort1.png", "sprites\\Resort2.png" }),
            new BuildingObject(new EntartainingBuliding(5, new Tolerance(.8, .9, .5, 1.2, .5)),500,
                new[] {"sprites\\School1.png", "sprites\\School2.png" }),
            new BuildingObject(new WorkingBuilding(100, new Person[]{ }), 800,
                new[] {"sprites\\Working1.png", "sprites\\Working2.png" }),
        };
        #endregion
        public Building Building { get; set; }
        public int cost;
        public List<Image> Textures = new List<Image> { };
        private readonly string[] texturesPathes;

        public BuildingObject(Building building, int cost, string[] textures)
        {
            this.cost = cost;
            Building = building;
            texturesPathes = textures;
            foreach (string texture in textures)
            Textures.Add(new Bitmap(Image.FromFile(texture), new Size(128, 128)));
        }

        public string GetString()
        {
            return Building.ToString();
        }

        public string GetDescription()
        {
            return Building.GetDescription() + $"\nСтоимость = {cost}";
        }

        public BuildingObject GetClone()
        {
            return new BuildingObject(this.Building.GetClone(), cost, texturesPathes);
        }
    }

    class GameModel
    {
        public BuildingObject[,] buildings;
        public readonly int height, width;
        public int PlayerMoney;
        public int Day;
        public double Happiness;
        private int livingBuildingsCount = 0;

        public GameModel(int height, int width)
        {
            this.height = height;
            this.width = width;
            PlayerMoney = 10000;
            buildings = new BuildingObject[height, width];
        }

        public bool Build(int row, int col, BuildingObject building)
        {
            if (building == null || InBounds(row, col) is false || buildings[row, col] != null || PlayerMoney - building.cost < 0)
                return false;
            else
            {
                buildings[row, col] = building;
                PlayerMoney -= building.cost;
            }
            for (int dr = -1; dr < 2; dr++)
                for (int dc = -1; dc < 2; dc++)
                    if (dc != 0 && dr != 0 && InBounds(row + dr, col + dc) && buildings[row + dr, col + dc] != null)
                    {
                        var watching = buildings[row + dr, col + dc];
                        if (building.Building.BuildingType == TypesOfBuilding.Living)
                        {
                            if (watching.Building.BuildingType == TypesOfBuilding.Entertaining)
                                building.Building.entartainings.Add(watching.Building);
                            else if (watching.Building.BuildingType == TypesOfBuilding.Working)
                                foreach (var rezident in building.Building.people)
                                    watching.Building.people.Add(rezident);
                            else if (watching.Building.BuildingType == TypesOfBuilding.Living)
                            {
                                ((LivingBuilding)watching.Building).neighbors.Add(building.Building);
                                ((LivingBuilding)building.Building).neighbors.Add(watching.Building);
                            }
                        }
                        else if (building.Building.BuildingType == TypesOfBuilding.Entertaining) {
                            if (watching.Building.BuildingType == TypesOfBuilding.Living)
                                watching.Building.entartainings.Add(building.Building);
                        }
                        else if (building.Building.BuildingType == TypesOfBuilding.Working)
                            if (watching.Building.BuildingType == TypesOfBuilding.Living)
                                foreach (var worker in ((LivingBuilding)watching.Building).people)
                                    ((WorkingBuilding)building.Building).People.Add(worker);
                    }
            return true;
        }

        public bool InBounds(int row, int col) {
            return (row >= 0 && row < height && col >= 0 && col < width);
        }

        public void ProgressStatement()
        {
            Happiness = 0;
            livingBuildingsCount = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    if (buildings[i, j] != null && buildings[i, j].Building.BuildingType == TypesOfBuilding.Living)
                        livingBuildingsCount++;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    if (buildings[i, j] != null)
                        if (buildings[i, j].Building.BuildingType == TypesOfBuilding.Living)
                        {
                            PlayerMoney += buildings[i, j].Building.GetMoney();
                            buildings[i, j].Building.ProgressStatement();
                            Happiness += buildings[i, j].Building.Happiness / livingBuildingsCount;
                        }
                        else if (buildings[i, j].Building.BuildingType == TypesOfBuilding.Working)
                            PlayerMoney += buildings[i, j].Building.GetMoney();
            Day ++;
        }
    }

    class MyButton: Label
    {
        public Color standartColor = Color.FromArgb(0, 0, 0, 255);
        public MyButton()
        {
            this.BackColor = Color.FromArgb(0, 0, 0, 255);
            this.MouseEnter += (sender, args) => this.BackColor = Color.FromArgb(128, 128, 128, 128);
            this.MouseLeave += (sender, args) => this.BackColor = standartColor;
        }
    }

    public partial class Form1 : Form
    {
        #region variables
        readonly int FPS = 60;
        GameModel model;
        GamePhases phase = GamePhases.Menu;

        //Load Timer vars
        int centerX, centerY, radius;

        Label debug = new Label
        {
            Text = "Привет. Я дебаг инфа",
            BackColor = Color.FromArgb(0, 0, 0, 255),
            ForeColor = Color.White,
            Visible = true
        };

        Label gameName = new Label {
                Text = "AI-Game",
                Font = new Font(FontFamily.GenericSansSerif, 25)
        };
        Label startGame = new Label
        {
            Text = "Начать игру",
            Font = new Font(FontFamily.GenericSerif, 22)
        };
        Label exitGame = new Label
        {
            Text = "Выйти из игры",
            Font = new Font(FontFamily.GenericSerif, 20)
        };

        Label sizesQuest = new Label
        {
            Text = "Размеры\nВысота и ширина",
            Font = new Font(FontFamily.GenericSerif, 20)
        };
        TextBox sizeHeight = new TextBox();
        TextBox sizeWidth = new TextBox();

        List<Control> menuLables = new List<Control>();

        MyButton continueButton = new MyButton();
        List<Control> pauseControls = new List<Control>();

        MyButton skipDay = new MyButton();
        List<Control> gameControls = new List<Control>();

        Font standartFont = new Font(FontFamily.GenericSerif, 18);


        Image menuBackground = new Bitmap("sprites\\MenuBackground.png");
        Bitmap menuImage;
        Image shopRoll = new Bitmap(Image.FromFile("sprites\\down.png"), new Size(128, 128));
        Image roadH = (Bitmap)Image.FromFile("sprites\\roadH.png");
        Image roadV = (Bitmap)Image.FromFile("sprites\\roadV.png");
        Image grass = (Bitmap)Image.FromFile("sprites\\grass.png");

        int CameraRow = 0;
        int CameraCol = 0;
        int startX = 64;
        int startY = 64;
        int mouseX = 0;
        int mouseY = 0;

        int FieldCol = 0;
        int FieldRow = 0;

        int ShopRow = 0;
        int ShopCol = 0;
        int ShopDelta = 0;
        int ShopSize = 0;

        ClickedMenus menu = ClickedMenus.NotClicked;
        int ClickedRow = -10;
        int ClickedCol = -10;
        BuildingObject selectedToBuild = null;

        string ClickString = "Вы играете в AI-Game";

        #endregion

        public Form1()
        {
            DoubleBuffered = true;
            ClientSize = new Size(1080, 720);

            menuImage = new Bitmap(menuBackground, ClientSize);

            #region menuElements
            
            //debug.Click += (sender, args) => is_loading = !is_loading;
            debug.Visible = false;
            Controls.Add(debug);

            Size nameSize = TextRenderer.MeasureText(gameName.Text, gameName.Font);
            gameName = new Label
            {
                Text = "AI-Game",
                Font = new Font(gameName.Font.FontFamily, 25),
                Size = nameSize,
                Location = new Point((ClientSize.Width - gameName.Size.Width), 0),
                BackColor = Color.FromArgb(0, 0, 0, 255),
                ForeColor = Color.White
            };
            Controls.Add(gameName);
            menuLables.Add(gameName);

            Size startGameSize = TextRenderer.MeasureText(startGame.Text, startGame.Font);
            startGame = new MyButton
            {
                Text = "Начать игру",
                Font = new Font(startGame.Font.FontFamily, 22),
                Size = startGameSize,
                Location = new Point((ClientSize.Width - startGame.Size.Width), 150),
                ForeColor = Color.White
            };
            startGame.Click += (sender, args) =>
            {
                Start();
            };
            Controls.Add(startGame);
            menuLables.Add(startGame);

            Size exitGameSize = TextRenderer.MeasureText(exitGame.Text, exitGame.Font);
            exitGame = new MyButton
            {
                Text = "Выйти из игры",
                Font = new Font(exitGame.Font.FontFamily, 20),
                Size = exitGameSize,
                Location = new Point((ClientSize.Width - exitGame.Size.Width), 400),
                ForeColor = Color.White
            };
            exitGame.Click += (sender, args) => Close();
            Controls.Add(exitGame);
            menuLables.Add(exitGame);

            var placeholder = "Размеры:\nвысота и ширина";
            sizesQuest = new Label {
                Text = "Размеры:\nвысота и ширина",
                Font = standartFont,
                Size = CalcSize(placeholder, standartFont),
                Location = new Point((ClientSize.Width - CalcSize(placeholder, standartFont).Width - 175), 175),
                BackColor = Color.FromArgb(0, 0, 0, 255),
                ForeColor = Color.White
            };
            Controls.Add(sizesQuest);
            menuLables.Add(sizesQuest);

            #endregion

            #region gameElements

            placeholder = "Пауза";
            var pause = new MyButton
            {
                Text = placeholder,
                Font = standartFont,
                Size = CalcSize(placeholder, standartFont),
                Location = new Point(0, 0),
                BackColor = Color.Brown
            };
            pause.standartColor = Color.Brown;
            pause.Click += (sender, args) => Pause();
            Controls.Add(pause);
            gameControls.Add(pause);

            placeholder = "Следующий день";
            skipDay = new MyButton
            {
                Text = placeholder,
                Font = standartFont,
                Size = new Size(300, 50),
                Location = new Point(ClientSize.Width - 300, ClientSize.Height - 50),
                BackColor = Color.Brown
            };
            skipDay.standartColor = Color.Brown;
            skipDay.Click += (sender, args) => { model.ProgressStatement(); ClickString = "Новый день в AI-Game"; menu = ClickedMenus.NotClicked; };
            skipDay.DoubleClick += (sender, args) => { model.ProgressStatement(); ClickString = "Новый день в AI-Game"; menu = ClickedMenus.NotClicked; };
            Controls.Add(skipDay);
            gameControls.Add(skipDay);

            placeholder = "День 1000";
            var day = new Label
            {
                Text = placeholder,
                Size = CalcSize(placeholder, standartFont),
                Location = new Point(200, 0),
                Font = standartFont,
                BackColor = Color.Brown
            };
            Controls.Add(day);
            gameControls.Add(day);

            placeholder = "Счастье 100%";
            var happiness = new Label
            {
                Text = placeholder,
                Size = CalcSize(placeholder, standartFont),
                Location = new Point(350, 0),
                Font = standartFont,
                BackColor = Color.Brown
            };
            Controls.Add(happiness);
            gameControls.Add(happiness);

            placeholder = "Бюджет = 1000000$";
            var money = new Label
            {
                Text = placeholder,
                Size = CalcSize(placeholder, standartFont),
                Location = new Point(525, 0),
                Font = standartFont,
                BackColor = Color.Brown
            };
            Controls.Add(money);
            gameControls.Add(money);

            #endregion

            #region pause Elements
            placeholder = "Продолжить";
            continueButton = new MyButton
            {
                Text = placeholder,
                Font = standartFont,
                Size = CalcSize(placeholder, standartFont),
                ForeColor = Color.White
            };
            continueButton.Click += (sender, args) => Continue();
            Controls.Add(continueButton);
            pauseControls.Add(continueButton);
            #endregion

            #region loadTimer
            var size = 10;

            var loadPhase = 0;
            var loadTimer = new Timer();
            loadTimer.Interval = 200;
            loadTimer.Tick += (sender, args) =>
            {
                if (loadPhase != 12)
                loadPhase++;
                else loadPhase = -3;
            };
            loadTimer.Start();
            #endregion

            #region gameTimer
            var gameTimer = new Timer();
            gameTimer.Interval = 1000 / FPS;
            gameTimer.Tick += (sender, args) =>
            {
                Invalidate();
                if (model != null)
                {
                    money.Text = $"Бюджет = {model.PlayerMoney}$";
                    day.Text = $"День {model.Day}";
                    happiness.Text = String.Format($"Счастье {Math.Round(model.Happiness * 100)}%");
                }
            };
            gameTimer.Start();
            #endregion

            #region framePainters
            //Black Background
            Paint += (sender, args) =>
            {
                args.Graphics.FillRectangle(Brushes.Black,
                    new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            };

            //Menu
            Paint += (sender, args) =>
            {
                if (phase == GamePhases.Menu || phase == GamePhases.Pause)
                {
                    args.Graphics.DrawImage(menuImage, new Point(0, 0));
                }
            };

            //Game
            Paint += (sender, args) =>
            {
                if (phase == GamePhases.Game )
                {
                    #region CalcCells
                    FieldCol = mouseX > (startX + 16) & (mouseX - startX - 16) / (128 + 16) <= model.width - 1 - CameraCol &
                    mouseY > (startY + 16) & (mouseY - startY - 16) / (128 + 16) <= model.height - 1 - CameraRow ?
                    (mouseX - startX - 16) / (128 + 16) : -10;

                    FieldRow = mouseX > (startX + 16) & (mouseX - startX - 16) / (128 + 16) <= model.width - 1 - CameraCol &
                    mouseY > (startY + 16) & (mouseY - startY - 16) / (128 + 16) <= model.height - 1 - CameraRow ?
                    (mouseY - startY - 16) / (128 + 16) : -10;

                    ShopCol = mouseX > (ClientSize.Width - 300 + 16) &
                    (mouseX - (ClientSize.Width - 300 - 16)) / (128 + 16) >= 0 &
                    (mouseX - (ClientSize.Width - 300 - 16)) / (128 + 16) <= 1 &
                    mouseY > 64 & (mouseY - 64) / (128 + 16) < ShopSize / 2 ?
                    (mouseX - (ClientSize.Width - 300 - 16)) / (128 + 16) : -10;

                    ShopRow = mouseX > (ClientSize.Width - 300 + 16) &
                    (mouseX - (ClientSize.Width - 300 - 16)) / (128 + 16) >= 0 &
                    (mouseX - (ClientSize.Width - 300 - 16)) / (128 + 16) <= 1 &
                    mouseY > 64 & (mouseY - 64) / (128 + 16) < ShopSize / 2 ?
                    (mouseY - 64) / (128 + 16) : -10;
                    #endregion

                    #region Background
                    TextureBrush textureRoadH = new TextureBrush(roadH);
                    textureRoadH.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                    TextureBrush textureRoadV = new TextureBrush(roadV);
                    textureRoadV.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;

                    TextureBrush grassBrush = new TextureBrush(grass);
                    grassBrush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;

                    args.Graphics.FillRectangle(grassBrush, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));

                    var HorizontalLength = Math.Min(model.width - CameraCol, ClientSize.Width / (128 + 16) + 16) * (128 + 16) + 16;
                    var VerticalLength = Math.Min(model.height - CameraRow, ClientSize.Height / (128 + 16) + 16) * (128 + 16) + 16;

                    for (int i = CameraCol - model.width + 2; i <= model.width - CameraCol; i++)
                    {
                        if (i >= 0)
                            args.Graphics.FillRectangle(textureRoadV, new Rectangle(i * (128 + 16) + startX,
                            startY, 16, VerticalLength));
                    }

                    for (int i = CameraRow - model.height + 2; i <= model.height - CameraRow; i++)
                    {
                        if (i >= 0)
                            args.Graphics.FillRectangle(textureRoadH, new Rectangle(startX,
                                i * (128 + 16) + startY, HorizontalLength, 16));
                    }
                    #endregion

                    if (menu != ClickedMenus.Shop)
                    {
                        args.Graphics.FillRectangle(Brushes.BlueViolet,
                        new Rectangle(startX + FieldCol * (128 + 16) + 16, startY + FieldRow * (128 + 16) + 16, 128, 128));
                        if (menu == ClickedMenus.Field)
                        {
                            args.Graphics.FillRectangle(Brushes.Green,
                                new Rectangle(startX + ClickedCol * (128 + 16) + 16, startY + ClickedRow * (128 + 16) + 16, 128, 128));
                        }
                    }
                    else if (menu == ClickedMenus.Shop)
                    {
                        if (model.InBounds(FieldRow + CameraRow, FieldCol + CameraCol) &&
                        model.buildings[FieldRow + CameraRow, FieldCol + CameraCol] == null)
                        {
                            args.Graphics.FillRectangle(Brushes.DarkGreen,
                                new Rectangle(startX + FieldCol * (128 + 16) + 16, startY + FieldRow * (128 + 16) + 16, 128, 128));
                            args.Graphics.DrawImage(selectedToBuild.Textures[1],
                                new Rectangle(startX + FieldCol * (128 + 16) + 16, startY + FieldRow * (128 + 16) + 16, 128, 128));
                        }
                        else
                            args.Graphics.FillRectangle(Brushes.DarkRed,
                                new Rectangle(startX + FieldCol * (128 + 16) + 16, startY + FieldRow * (128 + 16) + 16, 128, 128));
                    }

                    for (int row = 0; row < model.height; row++)
                    {
                        for (int col = 0; col < model.width; col++)
                            if (row >= CameraRow & col >= CameraCol)
                                if (model.buildings[row, col] != null)
                                    args.Graphics.DrawImage(model.buildings[row, col].Textures[1],
                                        new Point(startX + 16 + (col - CameraCol) * (128 + 16),
                                        startY + 16 + (row - CameraRow) * (128 + 16)));
                    }
                    
                    args.Graphics.FillRectangle(Brushes.White, new Rectangle(ClientSize.Width - 300, 0, 300, ClientSize.Height));
                    var BuyX = ClientSize.Width - 300 + 16;
                    var BuyY = 64;
                    ShopSize = (ClientSize.Height - 64 - 300) / (128 + 16) * 2;

                    args.Graphics.FillRectangle(Brushes.AliceBlue, new Rectangle(BuyX + ShopCol * (128 + 16), BuyY + ShopRow * (128 + 16), 128, 128));

                    if (menu == ClickedMenus.Shop)
                        args.Graphics.FillRectangle(Brushes.DarkBlue,
                        new Rectangle(ClientSize.Width - 300 + 16 + ClickedCol * (128 + 16), 64 + ClickedRow * (128 + 16), 128, 128));
                    for (int i = 0; i < ShopSize; i++)
                    {
                        if (i == ShopSize - 1)
                        {
                            args.Graphics.DrawImage(shopRoll, new Point(BuyX, BuyY));
                            break;
                        }
                        else
                        {
                            args.Graphics.DrawImage(BuildingObject.BuildingLibrary[(i + ShopDelta) % BuildingObject.BuildingLibrary.Count()].Textures[0], new Point(BuyX, BuyY));
                            BuyX += i % 2 == 0 ? (128 + 16) : -(128 + 16);
                            BuyY += i % 2 == 1 ? (128 + 16) : 0;
                        }
                    }

                    args.Graphics.DrawString(ClickString,
                        new Font(FontFamily.GenericSerif, 12), Brushes.Black, new Point(ClientSize.Width - 300 + 16, 64 + (ShopSize / 2) * (128 + 16)));
                }
            };

            Paint += (sender, args) =>
            {
                //if (is_loading)
                //{
                //    for (int i = 1; i <= loadPhase - 2; i++)
                //    {
                //        args.Graphics.TranslateTransform(centerX, centerY);
                //        args.Graphics.RotateTransform(i * 360f / 10);
                //        args.Graphics.FillEllipse(Brushes.Blue, radius - size / 2, -size / 2, size, size);
                //        args.Graphics.ResetTransform();
                //    }
                //}
            };
            #endregion

            #region controllers
            KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Escape)
                    if (phase == GamePhases.Game)
                    {
                        Pause();
                    }
                    else
                        this.Close();
                if (args.KeyCode == Keys.Up | args.KeyCode == Keys.W)
                    if (CameraRow >= 1)
                    CameraRow--;
                if (args.KeyCode == Keys.Down | args.KeyCode == Keys.S)
                    if (CameraRow < model.height - 1)
                        CameraRow++;
                if (args.KeyCode == Keys.Left | args.KeyCode == Keys.A)
                    if (CameraCol >= 1)
                        CameraCol--;
                if (args.KeyCode == Keys.Right | args.KeyCode == Keys.D)
                    if (CameraCol < model.width - 1)
                        CameraCol++;
                debug.Text = args.KeyCode.ToString();
            };

            SizeChanged += (sender, args) =>
            {
                ResetOfSizes();
            };

            MouseMove += (sender, args) =>
            {
                debug.Text = ((mouseY - startY - 16) / (128 + 16)).ToString();

                mouseX = args.Location.X;
                mouseY = args.Location.Y;
                //if (MousePosition.X / 10 == MousePosition.Y / 10)
                //    loadTimer.Stop();
                //else
                //    loadTimer.Start();
            };

            MouseClick += (sender, args) =>
            {
                ReactToClick();
            };

            MouseDoubleClick += (sender, args) =>
            {
                ReactToClick();
            };

            FormClosing += (sender, args) =>
            {
                    var result = MessageBox.Show("Действительно закрыть?", "",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                        args.Cancel = true;
            };
            #endregion

            Load += (sender, args) =>
            {
                Pause();
                foreach (var l in pauseControls)
                    l.Visible = false;
                ResetOfSizes();
            };

            ResizeEnd += (sender, args) =>
            {
                if (ClientSize.Width < 1080)
                    ClientSize = new Size(1080, ClientSize.Height);
                if (ClientSize.Height < 720)
                    ClientSize = new Size(ClientSize.Width, 720);
            };
        }

        private void Pause()
        {
            foreach (var l in gameControls)
                l.Visible = false;
            foreach (var l in menuLables)
                l.Visible = true;
            foreach (var l in pauseControls)
                l.Visible = true;

            sizeHeight = new TextBox();
            sizeHeight.Height = 50;
            sizeHeight.Width = 50;
            sizeHeight.MaxLength = 3;
            sizeHeight.Location = new Point((ClientSize.Width - 175), 200);
            Controls.Add(sizeHeight);
            menuLables.Add(sizeHeight);

            sizeWidth = new TextBox();
            sizeWidth.Height = 50;
            sizeWidth.Width = 50;
            sizeWidth.MaxLength = 3;
            sizeWidth.Location = new Point((ClientSize.Width - 75), 200);
            Controls.Add(sizeWidth);
            menuLables.Add(sizeWidth);

            phase = GamePhases.Pause;
            ResetOfSizes();
        }

        private void Start()
        {
            if (int.TryParse(sizeHeight.Text, out int height) && int.TryParse(sizeWidth.Text, out int width))
            {
                sizeHeight.Dispose();
                sizeWidth.Dispose();
                this.Focus();
                model = new GameModel(height, width);
                foreach (var l in menuLables)
                    l.Visible = false;
                foreach (var l in pauseControls)
                    l.Visible = false;
                foreach (var l in gameControls)
                    l.Visible = true;
                phase = GamePhases.Game;
                ResetOfSizes();
            }
        }

        private void Continue()
        {
            foreach (var l in menuLables)
                l.Visible = false;
            foreach (var l in pauseControls)
                l.Visible = false;
            foreach (var l in gameControls)
                l.Visible = true;
            sizeHeight.Dispose();
            sizeWidth.Dispose();
            phase = GamePhases.Game;
            ResetOfSizes();
        }

        private void ReactToClick()
        {
            if (ShopCol != -10)
            {
                if (menu != ClickedMenus.Shop && ShopRow * 2 + ShopCol + 1 == ShopSize)
                    ShopDelta++;
                else if (ClickedRow == ShopRow && ClickedCol == ShopCol)
                {
                    menu = ClickedMenus.NotClicked;
                    ClickedRow = -10;
                    ClickedCol = -10;
                }
                else if (menu != ClickedMenus.Shop || ShopRow * 2 + ShopCol + 1 != ShopSize)
                {
                    ClickedRow = ShopRow;
                    ClickedCol = ShopCol;
                    menu = ClickedMenus.Shop;
                    selectedToBuild = BuildingObject.BuildingLibrary[(ShopRow * 2 + ShopCol + ShopDelta) %
                        BuildingObject.BuildingLibrary.Count()];
                    ClickString = $"{selectedToBuild.GetDescription()}";
                }
            }
            else if (FieldCol != -10)
            {
                if (mouseX < ClientSize.Width - 300 && menu == ClickedMenus.Shop && model.InBounds(FieldRow + CameraRow, FieldCol + CameraCol) &&
                model.buildings[FieldRow + CameraRow, FieldCol + CameraCol] == null)
                {
                    if (Buy(FieldRow + CameraRow, FieldCol + CameraCol, selectedToBuild))
                        ClickString = "Куплено";
                    else
                        ClickString = "Не куплено";
                    menu = ClickedMenus.NotClicked;
                    ClickedRow = -10;
                    ClickedCol = -10;
                    selectedToBuild = null;
                }
                else if (model.InBounds(FieldRow + CameraRow, FieldCol + CameraCol) &&
                model.buildings[FieldRow + CameraRow, FieldCol + CameraCol] != null)
                {
                    ClickedRow = FieldRow;
                    ClickedCol = FieldCol;
                    ClickString = model.buildings[FieldRow + CameraRow, FieldCol + CameraCol].Building.ToString();
                    menu = ClickedMenus.Field;
                }
                else
                {
                    ClickString = "Вы играете в AI-Game";
                    menu = ClickedMenus.NotClicked;
                    ClickedRow = -10;
                    ClickedCol = -10;
                    selectedToBuild = null;
                }
            }
            else
            {
                ClickString = "Вы играете в AI-Game";
                menu = ClickedMenus.NotClicked;
                ClickedRow = -10;
                ClickedCol = -10;
                selectedToBuild = null;
            }
        }

        private bool Buy(int row, int col, BuildingObject building)
        {
            return model.Build(row, col, building.GetClone());
        }

        private void ResetOfSizes()
        {
            if (phase == GamePhases.Menu || phase == GamePhases.Pause)
            {
                gameName.Location = new Point((ClientSize.Width - gameName.Size.Width), 0);
                startGame.Location = new Point((ClientSize.Width - startGame.Size.Width), 150);
                continueButton.Location = new Point(ClientSize.Width - continueButton.Width, 250);
                exitGame.Location = new Point((ClientSize.Width - exitGame.Size.Width), 350);

                sizesQuest.Location = new Point((ClientSize.Width - sizesQuest.Size.Width - 175), 175);
                sizeHeight.Location = new Point((ClientSize.Width - 175), 200);
                sizeWidth.Location = new Point((ClientSize.Width - 75), 200);
            }
            if (phase == GamePhases.Game) {
                skipDay.Size = new Size(300, 50);
                skipDay.Location = new Point(ClientSize.Width - 300, ClientSize.Height - skipDay.Size.Height);
            }
            centerX = ClientSize.Width - ClientSize.Width / 17;
            centerY = ClientSize.Height - ClientSize.Height / 17;
            radius = Math.Min(ClientSize.Width / 17, ClientSize.Height / 17) / 2;
            if (ClientSize.Width != 0 & ClientSize.Height != 0)
            menuImage = new Bitmap(menuBackground, ClientSize);
        }

        public Size CalcSize(string text, Font font)
        {
            return TextRenderer.MeasureText(text, font);
        }
    }
}
