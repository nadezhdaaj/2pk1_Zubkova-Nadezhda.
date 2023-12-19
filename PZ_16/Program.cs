﻿using System.Threading.Channels;

namespace PZ_16
{

    internal class Program
    {
        // Параметры карты
        static int mapSize = 25; // Размер
        static char[,] map = new char[mapSize, mapSize];

        // Начальная позиция игрока
        static int playerY = mapSize / 2;
        static int playerX = mapSize / 2;

        // Количество элементов
        static byte enemies = 5; // Враги
        static byte buffs = 5; // Усиления
        static int health = 5;  // Аптечки   
        static int step = 0; // Общее количество шагов
        static int stepsave = 0; // После баффа

        // Характеристики игрока
        static int plHP = 50; // Здоровье 
        static int plDMG = 10; // Урон

        // Характеристики врагов
        static int enHP = 30; // Здоровье
        static int enDMG = 5; // Урон

        // Расположение текста в окне
        static int centerY = Console.WindowHeight / 2;
        static int centerX = (Console.WindowHeight / 2) - 15;

        //Сервисные переменные
        static string lastAction = "Начало игры"; // Окошко информации
        static bool bringbuff = false; // Поднятие баффа
        static int selectedMenuItem = 0; // Пункт меню

        static void Main(string[] args)
        {
            Console.Title = "Game";
            Menu();
            Move();
        }

        //MAP AND MOVE
        static void GenerationMap()
        {
            Random random = new Random();

            // Создание пустой карты
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (j == mapSize - 1)
                    {   // устанавливает положение курсора
                        Console.SetCursorPosition(i, j); // Решение проблемы с гранцией карты(переставление курсора)
                        map[i, j] = '_';  // Если текущая ячейка находится на границе карты (последний столбец), то устанавливается позиция курсора для перестановки курсора на новую строку
                    }
                    else
                    {
                        map[i, j] = '_';
                    }
                }
            }
            map[playerY, playerX] = 'P'; // В середину карты ставится игрок

            // Временные координаты для проверки занятости ячейки
            int x;
            int y;

            while (enemies > 0) // Добавление врагов
            {   // рандомная генерация 
                x = random.Next(0, mapSize);
                y = random.Next(0, mapSize);

                if (map[x, y] == '_')   // Если ячейка на этих координатах пуста , то в данную ячейку ставится символ врага ('E') и количество врагов уменьшается на 1
                {
                    map[x, y] = 'E';
                    enemies--;
                }
            }

            while (buffs > 0) // Добавление баффов
            {   // рандомная генерация
                x = random.Next(0, mapSize);
                y = random.Next(0, mapSize);

                if (map[x, y] == '_')   // Если ячейка на этих координатах пуста, то в нее ставится символ баффа ('B') и количество баффов уменьшается на 1
                {
                    map[x, y] = 'B';
                    buffs--;
                }
            }

            while (health > 0) // Добавление аптечек
            {   // рандомная генерация
                x = random.Next(0, mapSize);
                y = random.Next(0, mapSize);

                if (map[x, y] == '_')   // такм же образом добавляются аптечки
                {
                    map[x, y] = 'H';
                    health--;
                }
            }
            UpdateMap();
        }



        static void Victory()
        {
            for (int i = 0; i < mapSize; i++) // Проверка на наличие врагов
            {   // перебор элементов массива 
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == 'E')
                    {  // Если находится элемент 'E' функция завершается
                        return;
                    }
                }
            }

            Console.Clear();
            Centertext("Игра пройдена успешно :)", centerY);    // Если циклы завершаются без обнаружения врагов
            Centertext("Сделано " + step + " шагов", centerY + 1);
            Centertext("Нажмите Enter для выхода из игры", centerY + 2);

            ConsoleKeyInfo keyInfo;
            do
            {           // Этот код считывает клавишу на клавиатуре, пока пользователь не нажмет “Enter”. Затем, он закрывает консоль
                keyInfo = Console.ReadKey();
            } while (keyInfo.Key != ConsoleKey.Enter);

            Environment.Exit(0); // Выход
        }

        //BUFF, HEAL and FIGHT
        static void BuffUp() // Логика баффов
        {
            if (map[playerX, playerY] == 'B') // Проверяет, находится ли игрок на клетке с баффом
            {
                bringbuff = true; //  Если игрок находится на клетке с баффом
                stepsave = step; //Сохранение шага на котором взят бафф
                plDMG = plDMG * 2; // Урон увеличивается в 2 раза
                map[playerX, playerY] = '_'; // Решение проблемы "фантомного элемента" символ баффа на карте заменяется символом '_'
                lastAction = "Поднят бафф                            ";
            }
            if (stepsave == step - 20) // Расчитан на 20 шагов
            { // Если это условие выполняется, то бафф считается законченным
                bringbuff = false;
                plDMG = 10; // Возврат к изначальному урону
                lastAction = "Бафф закончился";
            }
        }

        static void Heal()
        {
            if (map[playerX, playerY] == 'H')
            {
                plHP = 50; // Лечение до максимума
                map[playerX, playerY] = '_'; // Решение проблемы "фантомного элемента"
                lastAction = "Вы подняли аптечку";
            }
        }
        static void Fight()
        {
            if (map[playerX, playerY] == 'E') // Если встать на врага
            {
                while (plHP > 0 && enHP > 0) // Пока оба живы
                {
                    enHP = enHP - plDMG;    // два персонажа наносят урон друг другу до тех пор, пока здоровье одного из них не станет равным нулю или меньше
                    plHP = plHP - enDMG;

                    if (plHP <= 0) // Если здоровье игрока кончилось, экран проигрыша
                    {
                        lastAction = "Бой был проигран. Здоровье: 0";
                        Console.Clear();
                        Centertext("Игра окончена", centerY);
                        Console.SetCursorPosition(centerX, centerY + 1);
                        Console.WriteLine("Нажмите Enter для возвращения в меню");

                        ConsoleKeyInfo keyInfo;
                        do
                        {   // после нажатия возвращение в меню
                            keyInfo = Console.ReadKey();
                        } while (keyInfo.Key != ConsoleKey.Enter);

                        Menu(); // Выход в меню
                    }

                    if (enHP <= 0) // Если враг побежден, то игрок остается в ячейке
                    { // здоровье игрока уменьшается 
                      // вместо врага на карте появляется символ '_'
                        map[playerX, playerY] = '_';
                        Console.SetCursorPosition(playerY, playerX);
                        Console.Write('_');
                        Console.SetCursorPosition(playerY, playerX);
                        Console.Write('P');
                        lastAction = "Вы победили врага и потеряли " + (50 - plHP) + " HP   ";
                    }
                    else // Анимация боя
                    {
                        // Если ни одно из вышеперечисленных условий не выполняется, то происходит анимация боя.
                        for (int i = 0; i < 3; i++) // Перебор символов анимации
                        {   
                            Console.SetCursorPosition(playerY, playerX);
                            Console.Write('|');
                            Thread.Sleep(60);
                            Console.SetCursorPosition(playerY, playerX);
                            Console.Write('/');
                            Thread.Sleep(60);
                            Console.SetCursorPosition(playerY, playerX);
                            Console.Write('-');
                            Thread.Sleep(60);
                        }   // После этого символ игрока обновляется на '_'
                        Console.Write('_');
                    }
                }
                enHP = 30; // Возврат здоровья для следующего игрока
            }
        }


        static void UpdateMap()  // Отображение заполненной карты на консоли
        {
            Console.Clear();
            for (int i = 0; i < mapSize; i++) //Запись карты в консоль
            {
                for (int j = 0; j < mapSize; j++)
                {
                    switch (map[i, j]) // Окраска элементов
                    {
                        case 'E':
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case 'B':
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case 'H':
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            break;
                    }

                    Console.Write(map[i, j]);   // элемент выводится на консоль
                    Console.ResetColor(); // После вывода каждого элемента цвет сбрасывается обратно в цвет по умолчанию 
                }
                Console.WriteLine();
            }
        }
        // MENU
        static void Menu() // Управление стрелочками и основной вызов меню
        {
            ConsoleKeyInfo key;
            do
            {
                Console.Clear();  // очищение экрана
                DisplayMenu();      // отображение меню на экране 

                key = Console.ReadKey(true);  // считываем нажатую клавишу и сохраняем её

                switch (key.Key)  // проверка нажатой клавиши 
                {    // переменная selectedMenuItem уменьшается на 1 и затем увеличивается на 3, чтобы остаться в пределах от 0 до 2.
                    case ConsoleKey.UpArrow:
                        selectedMenuItem = (selectedMenuItem - 1 + 3) % 3;
                        break;
                    case ConsoleKey.DownArrow:  // значение увеличивается на 1
                        selectedMenuItem = (selectedMenuItem + 1) % 3;
                        break;
                    case ConsoleKey.Enter:
                        HandleMenuSelection();  // обрабатывает выбор пользователя
                        break;
                }   // После обработки выбора пользователя, меню отображается снова
            } while (key.Key != ConsoleKey.Enter);
        }


        static void DisplayMenu() // Интерфейс
        {
            Centertext("THE GAME", centerY - 5);
            Console.ForegroundColor = (selectedMenuItem == 0) ? ConsoleColor.Blue : ConsoleColor.White; // Если равно, то голубой. Не равно - белый. 
            Centertext("1. Начать новую игру", centerY - 3);
            Console.ForegroundColor = (selectedMenuItem == 1) ? ConsoleColor.Blue : ConsoleColor.White;
            Centertext("2. Загрузка", centerY - 1);
            Console.ForegroundColor = (selectedMenuItem == 2) ? ConsoleColor.Blue : ConsoleColor.White;
            Centertext("3. Выход", centerY + 1);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void HandleMenuSelection() // Передача и открытие функций
        {
            Console.Clear();
            switch (selectedMenuItem)
            {
                case 0:
                    GenerationMap();
                    break;
                case 1:
                    Console.WriteLine("Загрузка...");
                    Loadgame();
                    UpdateMap();
                    break;
                case 2:
                    Console.WriteLine("Ждём вас в следующей игре!!!");
                    Environment.Exit(0);
                    break;
            }
            Console.ReadKey(true);
        }

        static void Move()
        {
            // Предыдущие координаты игрока
            int playerOldY;
            int playerOldX;

            while (true)
            {
                playerOldX = playerX;
                playerOldY = playerY;

                switch (Console.ReadKey().Key) // Управление через клавиатуру
                {
                    case ConsoleKey.UpArrow:
                        playerX--;
                        step++;
                        break;
                    case ConsoleKey.DownArrow:
                        playerX++;
                        step++;
                        break;
                    case ConsoleKey.LeftArrow:
                        playerY--;
                        step++;
                        break;
                    case ConsoleKey.RightArrow:
                        playerY++;
                        step++;
                        break;
                    case ConsoleKey.Escape: // Сохранение и выход из игры
                        Savegame();
                        Console.Clear();
                        Centertext("_Игра сохранена", centerY);
                        Centertext("Нажмите Enter чтобы выйти", centerY + 1);
                        Console.ReadLine();
                        return;
                }

                // Ограничение по границам карты
                if (playerX < 0) playerX = 0;
                if (playerY < 0) playerY = 0;
                if (playerX >= mapSize) playerX = mapSize - 1;
                if (playerY >= mapSize) playerY = mapSize - 1;

                Console.CursorVisible = false; // Скрытый курсор

                // Предыдущее положение игрока затирается
                map[playerOldY, playerOldX] = '_';
                Console.SetCursorPosition(playerOldY, playerOldX);
                Console.Write('_');

                // Обновленное положение игрока
                map[playerY, playerX] = 'P';
                Console.SetCursorPosition(playerY, playerX);
                Console.ForegroundColor = ConsoleColor.White; // Цвет игрока
                Console.Write('P');
                Console.ForegroundColor = ConsoleColor.White; // Цвет следа за игроком
                Console.SetCursorPosition(0, mapSize);

                // Применение функций боя, баффа, аптечки, победы
                Fight();
                BuffUp();
                Heal();
                Victory();

                // Отображение показателей
                int x, y;
                Console.SetCursorPosition(0, 25);
                Console.WriteLine($"Здоровье: {plHP}  ");
                Console.WriteLine($"Сила атаки: {plDMG}  ");
                Console.WriteLine($"Сделано шагов: {step}  ");
                GetPlayerPosition(out x, out y);
                Console.WriteLine($"x = {x}, y = {y}" + "  ");

                Console.SetCursorPosition(mapSize + 5, mapSize / 2);
                Console.Write("Последнее действие: " + lastAction);
            }
        }

        // SERVICE
        static void GetPlayerPosition(out int x, out int y) // Узнать позицию игрока
        {
            x = playerX;
            y = playerY;
        }

        static void Centertext(string text, int y) // Сделано для оформления по середине
        {
            int centerX = Console.WindowWidth / 2 - text.Length / 2;
            Console.SetCursorPosition(centerX, y);
            Console.WriteLine(text);
        }

        // SAVE
        static void Savegame() // Сохранение
        {
            string path = "save.txt"; // Создание текстового файла
            using (StreamWriter writer = new StreamWriter(path)) // Запись в него параметров
            {
                writer.WriteLine($"playerX={playerX}"); // координаты игрока
                writer.WriteLine($"playerY={playerY}"); // координаты игрока
                writer.WriteLine($"playerHP={plHP}");   // здоровье игрока
                writer.WriteLine($"playerStrong={plDMG}");  // сила атаки игрока
                writer.WriteLine($"playerStepCount={step}");    // количество сделанных игроком шагов
                writer.WriteLine($"enemyHP={enHP}");    // здоровье врага
                writer.WriteLine($"hasBuff={bringbuff}");   // наличие баффа 
                writer.WriteLine($"buffStep={stepsave}");   // количество сделанных с шагов с баффом 

                for (int i = 0; i < mapSize; i++) // Запись карты
                {
                    for (int j = 0; j < mapSize; j++)   // Перебор элементов в массиве map и запись их в файл
                    {
                        if (map[i, j] == 'P')
                        {                         //  Если элемент равен символу 'P', то он заменяется на символ '_', перед записью в файл. 
                            map[i, j] = '_';        //  С целью скрыть позицию игрока в сохранении, чтобы не было возможности изменить его позицию напрямую в файле.
                        }
                        writer.Write(map[i, j]);
                    }
                    writer.WriteLine();
                }
            }
        }
        
            static void Loadgame() // Загрузка
            {
                string path = "save.txt"; // Определяем путь

                if (File.Exists(path)) // Если существует
                {
                    string[] lines = File.ReadAllLines(path); // Передача файлов с документа в игру

                    if (lines.Length >= mapSize)    // Проверка, что количество строк в файле сохранения достаточно для загрузки игры 
                {       // Если проверка проходит, то происходит парсинг строк файла сохранения и присваивание значений переменным
                    if (int.TryParse(lines[0].Split('=')[1], out int loadedPlayerX) &&
                        int.TryParse(lines[1].Split('=')[1], out int loadedPlayerY) &&
                        int.TryParse(lines[2].Split('=')[1], out int loadedPlayerHP) &&
                        int.TryParse(lines[3].Split('=')[1], out int loadedPlayerStrong) &&
                        int.TryParse(lines[4].Split('=')[1], out int loadedPlayerStepCount) &&
                        int.TryParse(lines[5].Split('=')[1], out int loadedEnemyHP) &&
                        bool.TryParse(lines[6].Split('=')[1], out bool loadedHasBuff) &&
                        int.TryParse(lines[7].Split('=')[1], out int loadedBuffStep))
                        {
                            playerX = loadedPlayerX;
                            playerY = loadedPlayerY;
                            plHP = loadedPlayerHP;
                            plDMG = loadedPlayerStrong;
                            step = loadedPlayerStepCount;
                            stepsave = loadedBuffStep;
                            enHP = loadedEnemyHP;
                            bringbuff = loadedHasBuff;
                        // Инициализация игрового поля и заполнение его значениями из файла сохранения
                        for (int i = 0; i < mapSize; i++)
                            {
                                for (int j = 0; j < mapSize; j++)
                                {
                                    map[i, j] = '_';
                                }
                            }

                            for (int i = 0; i < mapSize; i++)
                            {
                                for (int j = 0; j < mapSize; j++)
                                {
                                    map[i, j] = lines[i + 8][j];
                                }
                            }
                        // Координаты игрока обозначаются символом 'P'
                        map[playerX, playerY] = 'P'; 

                            Console.Clear();
                            UpdateMap(); //Вывод на консоль
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при чтении файла сохранения.");
                    }
                }
                else
                {
                    Console.WriteLine("К сожалению, файл сохранения не найден.");
                }
            }
        }
    }