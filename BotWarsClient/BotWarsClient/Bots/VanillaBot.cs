using System;
using System.Collections.Generic;
using System.Linq;

namespace BotWarsClient.Bots
{
    public class VanillaBot : BotBaseClass
    {
        private static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));

        public string OpponentName { get; set; }

        public int Health { get; set; }

        public int Flips { get; set; }

        public int Fuel { get; set; }

        public int FlipOdds { get; set; }

        public int ArenaSize { get; set; }

        public char Direction { get; set; }

        public bool Flipped { get; set; }

        public Move OpponentLastMove { get; set; }

        public bool OpponentFlipped { get; set; }

        public BotPosition[] Position { get; set; }

        public bool IsInitialized { get; set; }

        public IList<Move> TheirMoves { get; set; }

        public int GetPosition(BotPosition position)
        {
            for (var a = 0; a < this.Position.Length; a++)
            {
                if (this.Position[a] == position)
                {
                    return a;
                }
            }
            return -1;
        }

        public void SetPosition(BotPosition position, int index)
        {
            if (index < 0 && index >= this.Position.Length)
            {
                throw new InvalidOperationException("Position is out of range.");
            }
            if (this.Position[index] != BotPosition.None)
            {
                throw new InvalidOperationException("Space is occupied.");
            }
            this.Position[this.GetPosition(position)] = BotPosition.None;
            this.Position[index] = position;
        }

        public int GetProximityToEdge()
        {
            var position = this.GetPosition(BotPosition.Us);
            int proximity = default(int);
            switch (this.Direction)
            {
                case 'l':
                    proximity = (this.ArenaSize - position) - 1;
                    break;
                case 'r':
                    proximity = position;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Console.Out.WriteLine(String.Format("(Edge Proximity) Arena Size: {0}, Our position: {1},  Proximity {2}", this.ArenaSize, position, proximity));
            return proximity;
        }

        public int GetProximityToOpponent()
        {
            var ourPosition = this.GetPosition(BotPosition.Us);
            var theirPosition = this.GetPosition(BotPosition.Them);
            var proximity = Math.Abs(ourPosition - theirPosition);
            Console.Out.WriteLine(String.Format("(Opponent Proximity) Arena Size: {0}, Our position: {1}, Their Position: {2}, Proximity {3}", this.ArenaSize, ourPosition, theirPosition, proximity));
            return proximity;
        }

        public int MoveNumber { get; set; }

        public VanillaBot()
        {
            this.Direction = 'r';
            this.Position = new BotPosition[100];
        }

        /// <summary>
        /// Tell the server what we'd like to do in our move
        /// </summary>
        public override Move GetMove()
        {
            if (!this.IsInitialized)
            {
                Console.Out.WriteLine("Not initialized.");
                return Move.AttackWithAxe;
            }

            this.PrintPosition();

            this.MoveNumber++;
            if (this.Flipped)
            {
                if (this.Flips > 0)
                {
                    Console.Out.WriteLine("Self righting.");
                    this.Flips--;
                    this.Flipped = false;
                    return Move.Flip;
                }
                else
                {
                    Console.Out.WriteLine("Cannot self right!");
                    return Move.Invalid;
                }
            }

            if (this.MoveNumber == 0 && this.Fuel > 0)
            {
                Console.Out.Write("It's the first move, flame thrower is safe.");
                this.Fuel--;
                return Move.FlameThrower;
            }

            try
            {
                if (this.GetProximityToEdge() > 1 && this.AreTheyUsing(Move.Flip, Move.Shunt))
                {
                    Console.Out.Write("They're using flips and shunts and we have some space, backing off.");
                    this.MoveUsBackward();
                    return Move.MoveBackwards;
                }

                if (this.AreTheyUsing(Move.Shunt) && this.Flips > 0 && this.IsInRangeOfAttack(Move.Flip))
                {
                    Console.Out.Write("They're using shunts, flipping.");
                    this.Flips--;
                    return Move.Flip;
                }

                if (this.GetProximityToEdge() <= 1)
                {
                    Console.Out.WriteLine("We're close the the edge, attempting to reclaim ground.");
                    if (this.CanMove(Move.MoveForwards))
                    {
                        Console.Out.WriteLine("Moving forwards.");
                        this.MoveUsForward();
                        return Move.MoveForwards;
                    }
                    else
                    {
                        if (this.GetProximityToEdge() > 0 && (this.MoveNumber % 2 == 0))
                        {
                            if (this.Fuel > 0 && this.IsInRangeOfAttack(Move.FlameThrower))
                            {
                                Console.Out.WriteLine("We have fuel and their in range, using flame thrower.");
                                this.Fuel--;
                                return Move.FlameThrower;
                            }
                            else
                            {
                                Console.Out.WriteLine("Axing.");
                                return Move.AttackWithAxe;
                            }
                        }
                        else if (this.IsInRangeOfAttack(Move.Shunt))
                        {
                            Console.Out.WriteLine("Shunting.");
                            this.MoveThemBackward();
                            this.MoveUsForward();
                            return Move.Shunt;
                        }
                    }
                }

                if (this.Fuel > 0 && this.IsInRangeOfAttack(Move.FlameThrower))
                {
                    Console.Out.WriteLine("We have fuel and their in range, using flame thrower.");
                    this.Fuel--;
                    return Move.FlameThrower;
                }

                if (this.IsInRangeOfAttack(Move.AttackWithAxe))
                {
                    Console.Out.WriteLine("Close quarters, Axing.");
                    return Move.AttackWithAxe;
                }

                if (this.CanMove(Move.MoveForwards))
                {
                    Console.Out.WriteLine("Closing in.");
                    this.MoveUsForward();
                    return Move.MoveForwards;
                }
                else if (this.IsInRangeOfAttack(Move.Shunt))
                {
                    Console.Out.WriteLine("Shunting.");
                    this.MoveThemBackward();
                    this.MoveUsForward();
                    return Move.Shunt;
                }
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine(e.Message);
            }
            //Fuck it.
            return Move.AttackWithAxe;
        }

        protected virtual void PrintPosition()
        {
            System.Console.Out.Write("Positions : [");
            for (var a = 0; a < this.Position.Length; a++)
            {
                switch (this.Position[a])
                {
                    case BotPosition.None:
                        System.Console.Out.Write("----");
                        break;
                    case BotPosition.Us:
                        System.Console.Out.Write("-Us-");
                        break;
                    case BotPosition.Them:
                        System.Console.Out.Write("Them");
                        break;
                }
            }
            System.Console.Out.WriteLine("]");
        }

        public override void SetStartValues(string opponentName, int health, int arenaSize, int flips, int flipOdds, int fuel, char direction, int startIndex)
        {
            OpponentName = opponentName;
            Health = health;
            ArenaSize = arenaSize;
            Flips = flips;
            FlipOdds = flipOdds;
            Fuel = fuel;
            Direction = direction;
            Flipped = false;
            OpponentFlipped = false;

            this.Position = new BotPosition[arenaSize];
            this.Position[startIndex] = BotPosition.Us;

            this.TheirMoves = new List<Move>();

            if (arenaSize % 2 == 0)
            {
                if (startIndex < arenaSize / 2)
                {
                    this.Position[startIndex + 1] = BotPosition.Them;
                }
                else
                {
                    this.Position[startIndex - 1] = BotPosition.Them;
                }
            }
            else
            {
                if (startIndex < arenaSize / 2)
                {
                    this.Position[startIndex + 2] = BotPosition.Them;
                }
                else
                {
                    this.Position[startIndex - 2] = BotPosition.Them;
                }
            }

            this.MoveNumber = -1;

            this.IsInitialized = true;

            base.SetStartValues(opponentName, health, arenaSize, flips, flipOdds, fuel, direction, startIndex);
        }

        public override void CaptureOpponentsLastMove(Move lastOpponentsMove)
        {
            try
            {
                switch (lastOpponentsMove)
                {
                    case Move.MoveForwards:
                        if (!this.CanMove(Move.MoveForwards))
                        {
                            this.MoveUsBackward();
                        }
                        this.MoveThemForward();
                        break;
                    case Move.MoveBackwards:
                        this.MoveThemBackward();
                        break;

                    case Move.Shunt:
                        //It's complicated.
                        this.MoveUsBackward();
                        this.MoveThemForward();
                        break;
                }
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine(e.Message);
            }
            this.OpponentLastMove = lastOpponentsMove;
            this.TheirMoves.Add(lastOpponentsMove);
        }

        protected virtual bool AreTheyUsing(params Move[] moves)
        {
            var count = this.TheirMoves.Take(10).Count(move => moves.Contains(move));
            return count > 5;
        }

        public void MoveUsForward()
        {
            this.MoveForward(BotPosition.Us);
        }

        public void MoveThemForward()
        {
            this.MoveBackward(BotPosition.Them);
        }

        protected virtual void MoveForward(BotPosition position)
        {
            switch (this.Direction)
            {
                case 'l':
                    this.SetPosition(position, this.GetPosition(position) - 1);
                    break;
                case 'r':
                    this.SetPosition(position, this.GetPosition(position) + 1);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void MoveUsBackward()
        {
            this.MoveBackward(BotPosition.Us);
        }

        public void MoveThemBackward()
        {
            this.MoveForward(BotPosition.Them);
        }

        protected virtual void MoveBackward(BotPosition position)
        {
            switch (this.Direction)
            {
                case 'l':
                    this.SetPosition(position, this.GetPosition(position) + 1);
                    break;
                case 'r':
                    this.SetPosition(position, this.GetPosition(position) - 1);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual bool CanMove(Move move)
        {
            var proximity = this.GetProximityToOpponent();
            switch (move)
            {
                case Move.MoveForwards:
                    return proximity > 1;
                case Move.MoveBackwards:
                    //We will never do this.
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual bool IsInRangeOfAttack(Move move)
        {
            var proximity = this.GetProximityToOpponent();
            switch (move)
            {
                case Move.AttackWithAxe:
                    return proximity <= 1;
                case Move.FlameThrower:
                    return proximity <= 2;
                case Move.Flip:
                    return proximity <= 1 && (this.FlipOdds + Random.Next(50)) > 50;
                case Move.Shunt:
                    return proximity <= 1;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void SetFlippedStatus(bool flipped)
        {
            this.Flipped = flipped;
        }

        public override void SetOpponentFlippedStatus(bool opponentFlipped)
        {
            this.OpponentFlipped = opponentFlipped;
        }
    }
}
