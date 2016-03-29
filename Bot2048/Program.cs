using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot2048
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Enter board state separated by comma, left to right, top to bottom:");
			string inputState = Console.ReadLine();
			Board board = ParseBoard(inputState);
			if (board == null)
			{
				Console.WriteLine("Could not parse board state");
				return;
			}

			Console.Write("Enter simulation time per move in ms: ");
			string simulTimeInput = Console.ReadLine();
			int simulTimeMs;
			if (!int.TryParse(simulTimeInput, out simulTimeMs))
			{
				Console.WriteLine("Could not parse simulation time");
				return;
			}

			Random rand = new Random();
			while (true)
			{
				Console.WriteLine("Current board");
				Console.WriteLine(PrettyPrint(board.GetState()));

				Direction move = SearchForMove(board, simulTimeMs);
				Console.WriteLine("Direction: {0}", move);
				Console.WriteLine("-------------");
				if (!board.Move(move))
					break;
				board.SpawnRandom(rand);
			}

			Console.WriteLine("Game finished");
			Console.ReadLine();
		}

		private static Direction SearchForMove(Board board, int maxMs)
		{
			Stopwatch timer = Stopwatch.StartNew();
			long[] totalMoves = new long[4];
			long[] totalGames = new long[4];
			Random bigRand = new Random();
			ThreadLocal<Random> randLocal = new ThreadLocal<Random>(() =>
			                                                        {
				                                                        lock (bigRand)
					                                                        return new Random(bigRand.Next());
			                                                        });
			Parallel.ForEach(ProduceDirections(timer, maxMs),
			                 dir =>
			                 {
				                 Random rand = randLocal.Value;

				                 Board startBoard = board.Clone();
				                 if (!startBoard.Move(dir))
					                 return;

				                 Board game = new Board();
				                 long moves = 0;
				                 long games = 0;
				                 for (int i = 0; i < 100; i++)
				                 {
					                 game.SetFromBoard(startBoard);
					                 do
					                 {
						                 game.SpawnRandom(rand);
						                 moves++;
					                 } while (RandomMove(game, rand));

					                 games++;
				                 }

				                 Interlocked.Add(ref totalMoves[(int)dir], moves);
				                 Interlocked.Add(ref totalGames[(int)dir], games);
			                 });


			double bestAvg = double.MinValue;
			int bestDir = -1;
			for (int i = 0; i < 4; i++)
			{
				double avg = totalMoves[i]/(double)totalGames[i];
				if (avg > bestAvg)
				{
					bestAvg = avg;
					bestDir = i;
				}
			}

			return (Direction)bestDir;
		}

		private static bool RandomMove(Board game, Random rand)
		{
			for (int i = 0; i < 50; i++)
			{
				Direction dir = (Direction)rand.Next(4);
				if (game.Move(dir))
					return true;
			}

			return false;
		}

		private static IEnumerable<Direction> ProduceDirections(Stopwatch timer, int maxMs)
		{
			while (timer.ElapsedMilliseconds < maxMs)
			{
				yield return Direction.Up;
				yield return Direction.Right;
				yield return Direction.Down;
				yield return Direction.Left;
			}
		}

		private static Board ParseBoard(string input)
		{
			string[] split = input.Split(',');
			if (split.Length != 4*4)
				return null;

			Board board = new Board();
			for (uint y = 0; y < 4; y++)
			{
				for (uint x = 0; x < 4; x++)
				{
					int val;
					if (!int.TryParse(split[y*4 + x], out val))
						return null;

					board.Set(x, y, val);
				}
			}

			return board;
		}

		private static string PrettyPrint(int[] values)
		{
			int[] paddingRequired = new int[4];
			for (int x = 0; x < 4; x++)
			{
				for (int y = 0; y < 4; y++)
				{
					paddingRequired[x] = Math.Max(paddingRequired[x], values[y*4 + x].ToString().Length);
				}
			}

			StringBuilder sb = new StringBuilder();
			for (int y = 0; y < 4; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					sb.Append(values[y*4 + x].ToString().PadLeft(paddingRequired[x]));
					if (x != 3)
						sb.Append(", ");
				}

				if (y != 3)
					sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
