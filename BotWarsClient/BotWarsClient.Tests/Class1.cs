using BotWarsClient.Bots;
using NUnit.Framework;
using System;

namespace BotWarsClient.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TracksPositionsLeft()
        {
            var bot = new VanillaBot();
            bot.SetStartValues(string.Empty, 100, 13, 0, 0, 0, 'l', 7);
            Assert.AreEqual(7, bot.GetPosition(BotPosition.Us));
            Assert.AreEqual(5, bot.GetPosition(BotPosition.Them));

            bot.MoveUsForward();

            Assert.AreEqual(6, bot.GetPosition(BotPosition.Us));
            Assert.AreEqual(6, bot.GetProximityToEdge());

            Assert.Throws<InvalidOperationException>(() => bot.MoveUsForward());

            bot.MoveThemBackward();

            Assert.AreEqual(4, bot.GetPosition(BotPosition.Them));

            bot.MoveUsForward();

            Assert.AreEqual(5, bot.GetPosition(BotPosition.Us));
        }

        [Test]
        public void TracksPositionsRight()
        {
            var bot = new VanillaBot();
            bot.SetStartValues(string.Empty, 100, 13, 0, 0, 0, 'r', 5);
            Assert.AreEqual(5, bot.GetPosition(BotPosition.Us));
            Assert.AreEqual(7, bot.GetPosition(BotPosition.Them));

            bot.MoveUsForward();

            Assert.AreEqual(6, bot.GetPosition(BotPosition.Us));

            Assert.Throws<InvalidOperationException>(() => bot.MoveUsForward());

            bot.MoveThemBackward();

            Assert.AreEqual(8, bot.GetPosition(BotPosition.Them));

            bot.MoveUsForward();

            Assert.AreEqual(7, bot.GetPosition(BotPosition.Us));
        }

        [Test]
        public void PicksCorrectAttack()
        {
            var bot = new VanillaBot();
            bot.SetStartValues(string.Empty, 100, 13, 0, 0, 0, 'l', 7);
        }

        [TestCase(13, 'l', 7, 7, 5)]
        [TestCase(13, 'r', 5, 5, 7)]
        [TestCase(14, 'l', 7, 7, 6)]
        [TestCase(14, 'r', 6, 6, 7)]
        public void InitialPositions(int arenaSize, char direction, int startIndex, int us, int them)
        {
            var bot = new VanillaBot();
            bot.SetStartValues(string.Empty, 100, arenaSize, 0, 0, 0, direction, startIndex);
            Assert.AreEqual(us, bot.GetPosition(BotPosition.Us));
            Assert.AreEqual(them, bot.GetPosition(BotPosition.Them));
        }
    }
}
