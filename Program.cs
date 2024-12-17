using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

namespace Laba3
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                List<Player> Players = new List<Player>();
                int QuantityPlayers = 0;
                Console.WriteLine("0 - продолжить игру\n1 - начать новую игру: ");
                int s = Convert.ToInt32(Console.ReadLine());
                Console.Clear();
                //Считывание с файла сохраненной игры
                if (s == 0)
                {
                    using (FileStream fs = new FileStream("user.json", FileMode.OpenOrCreate))
                    {
                        if (fs.Length == 0) throw new Exception("Нет сохраненных игр");
                        List<Player>? Playerr = await JsonSerializer.DeserializeAsync<List<Player>>(fs);
                        Players = Playerr;
                        QuantityPlayers = Players.Count;
                    }
                }
                //Создание новой игры, ввод количества участников
                else
                {
                    Players = new List<Player>();

                    string name;
                    Console.WriteLine("Введите количество игроков(от 2 до 6): ");
                    QuantityPlayers = Convert.ToInt32(Console.ReadLine());
                    if (QuantityPlayers >= 2 && QuantityPlayers <= 6)
                    {
                        for (int i = 0; i < QuantityPlayers; i++)
                        {
                            Console.WriteLine($"Введите имя {i + 1}-го игрока: ");
                            name = Console.ReadLine();
                            Player player = new Player(name);
                            Players.Add(player);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверное количество игроков.");
                        Environment.Exit(0);
                    }
                }
                Console.Clear();

                for (int i = 0; i < QuantityPlayers; i++)
                {
                    //Если у игрока не пропущен ход
                    if (Players[i].Motion != 0)
                    {
                        Console.WriteLine("**Нажмите Escape, чтобы выйти из игры**");
                        Console.WriteLine("Позиции игроков:");
                        for (int j = 0; j < QuantityPlayers; j++)
                        {
                            Console.WriteLine($"{Players[j].Name}: {Players[j].Position}-е поле");
                        }
                        Console.WriteLine($"\nИгрок {Players[i].Name}, сделайте ход.\nНажмите Enter, чтобы бросить кубик");

                        var key = Console.ReadKey().Key;
                        while (key != ConsoleKey.Enter)
                        {
                            //завершение игры
                            if (key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("11 - сохранить игру, 0 - не сохранять:");
                                int safe = Convert.ToInt32(Console.ReadLine());
                                switch (safe)
                                {
                                    case 0:
                                        //JsonObject.Clear();
                                        break;
                                    //Запись сохраненной игры в файл
                                    case 1:
                                        using (FileStream fs = new FileStream("user.json", FileMode.OpenOrCreate))
                                        {
                                            await JsonSerializer.SerializeAsync<List<Player>>(fs, Players);
                                            Console.WriteLine($"Data has been saved to file");
                                            
                                        }
                                        break;
                                    default: 
                                        Console.WriteLine("неверно введен выбор.");
                                        break;
                                }
                                Environment.Exit(0);
                            }
                            key = Console.ReadKey().Key;
                        }
                        int Positions = Players[i].Move();
                        Players[i].Motion--;
                        if (Positions == 1) Console.WriteLine($"Вы сделали {Positions} шаг!");
                        else if (Positions >= 2 && Positions <= 4) Console.WriteLine($"Вы сделали {Positions} шага!");
                        else Console.WriteLine($"Вы сделали {Positions} шагов!");
                        int step = Players[i].Step(Positions);
                        Random random = new Random();
                        int RandomPlayer = random.Next(QuantityPlayers);
                        while (i == RandomPlayer)
                        {
                            RandomPlayer = random.Next(QuantityPlayers);
                        }
                        int pos = Players[RandomPlayer].Position;
                        Players[i].ChangePosition(Positions, step, pos);
                    }
                    //Если у игрока пропущен ход, добавление одного хода для следующего круга
                    else
                    {
                        Players[i].Motion++;
                        if (i == QuantityPlayers - 1) i = -1;
                        continue;
                    }
                    //Определение победителя
                    if (Players[i].Position >= 100)
                    {
                        Console.WriteLine("Вы выиграли!!!");
                        break;
                    }
                    //Если у игрока двойной ход, переходим к нему же на следующем ходе
                    if (Players[i].Motion == 1) i--;
                    //Иначе добавляем один ход для следующего круга
                    else Players[i].Motion++;
                    //Если игрок был последний по счету в круге, переходим к первому
                    if (i == QuantityPlayers - 1) i = -1;
                    Console.WriteLine("Нажмите Enter, чтобы перейти к следующему игроку:");
                    var _key = Console.ReadKey().Key;
                    while (_key != ConsoleKey.Enter)
                    {
                        //Завершение игры
                        if (_key == ConsoleKey.Escape)
                        {
                            Console.WriteLine("11 - сохранить игру, 0 - не сохранять:");
                            int safe = Convert.ToInt32(Console.ReadLine());
                            switch (safe)
                            {
                                case 0: break;
                                case 1:
                                    using (FileStream fs = new FileStream("user.json", FileMode.OpenOrCreate))
                                    {
                                        //fs.Seek(0, SeekOrigin.End);
                                        await JsonSerializer.SerializeAsync<List<Player>>(fs, Players);
                                        Console.WriteLine($"Data has been saved to file");

                                    }
                                    break;
                                default:
                                    Console.WriteLine("неверно введен выбор.");
                                    break;
                            }
                            Environment.Exit(0);
                        }
                        _key = Console.ReadKey().Key;
                    }
                    Console.Clear();
                }
                Console.WriteLine("Игра окончена.");
            }

            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
    // Игрок
    public class Player
    {
        private string name;
        private int position;
        private int motion;
        //Текущая позиция игрока
        public int Position
        {
            get { return position; }
            set
            {
                if (value < 0) throw new Exception("Такой позиции нет.");
                else position = value;
            }
        }
        //Имя игрока
        public string Name
        {
            get { return name; }
            set
            {
                if (value.Length <= 0) throw new Exception("Неверно введено имя игрока.");
                else name = value;
            }
        }
        //Количество ходов
        public int Motion
        {
            get { return motion; }
            set
            {
                if (value < -1 || value > 2) throw new ArgumentException();
                motion = value;
            }
        }
        public Player(string name = "player")
        {
            Name = name;
            Position = 0;
            motion = 1;
        }
        //Количество шагов за ход
        public int Move()
        {
            Random random = new Random();
            int Position = random.Next(7);
            while (Position == 0) Position = random.Next(7);
            return Position;
        }
        //Специальные ходы
        public int Step(int move)
        {
            int step = 0;
            if (Position + move >= 100 && ((Position + move) % 13 != 0)) return step;
            if ((Position + move) % 15 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, перемещаетесь на 4 шага вперёд!");
                step = 15;
            }
            else if ((Position + move) % 13 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, перемещаетесь на 4 шага назад.");
                step = 13;
            }
            else if ((Position + move) % 21 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, сделайте дополнительный ход!");
                step = 21;
            }
            else if ((Position + move + 1) % 21 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, пропускаете следующий ход.");
                step = 20;
            }
            else if ((Position + move) % 33 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, перемещаетесь в начало.");
                step = 33;
            }
            else if ((Position + move + 4) % 33 == 0)
            {
                Console.WriteLine($"Вы попали на {Position + move}-е поле, перемещаетесь к случайному игроку.");
                step = 29;
            }
            return step;
        }
        //Изменение позиции игрока
        public void ChangePosition(int move, int step, int pos)
        {
            Position += move;
            if (step == 15) Position += 4;
            else if (step == 13) Position -= 4;
            else if (step == 21) Motion++;
            else if (step == 20) Motion--;
            else if (step == 33) Position = 0;
            else if (step == 29) Position = pos;
            if (Position <= 100) Console.WriteLine($"Вы переместились на {Position}-е поле!");
        }
    }
}