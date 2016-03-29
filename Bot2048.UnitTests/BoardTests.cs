using System;
using System.Linq;
using System.Text;
using Xunit;

namespace Bot2048.UnitTests
{
	public class BoardTests
	{
		private Board CreateBoard(params int[] values)
		{
			Board board = new Board();
			int i = 0;
			for (uint y = 0; y < 4; y++)
			{
				for (uint x = 0; x < 4; x++)
				{
					if (i < values.Length)
						board.Set(x, y, values[i++]);
				}
			}

			return board;
		}

		private bool BoardMatches(Board board, params int[] values)
		{
			return board.GetState().SequenceEqual(values);
		}

		// Assert that moving 'board' upwards yields 'values',
		// and then rotate board and values for all directions.
		private void AssertMoves(Board board, params int[] values)
		{
			Direction[] dirsInOrder = {Direction.Up, Direction.Right, Direction.Down, Direction.Left};
			foreach (Direction dir in dirsInOrder)
			{
				int[] prevState = board.GetState();
				bool test = board.Move(dir);
				bool expected = values != null;
				if (test != expected)
				{
					StringBuilder message = new StringBuilder();
					message.AppendFormat("Move {0} when trying to move the following board {1}",
					                     expected ? "failed" : "did not fail", dir);
					message.AppendLine();
					message.Append(PrettyPrint(prevState));

					Assert.True(false, message.ToString());
				}

				if (values != null && !BoardMatches(board, values))
				{
					StringBuilder message = new StringBuilder();
					message.AppendLine("Board");
					message.AppendLine(PrettyPrint(prevState));
					message.AppendLine("did not match expected board when moving " + dir);
					message.AppendLine();
					message.AppendLine("Expected:");
					message.AppendLine(PrettyPrint(values));
					message.AppendLine();
					message.AppendLine("Actual:");
					message.AppendLine(PrettyPrint(board.GetState()));

					Assert.True(false, message.ToString());
				}

				board = new Board(Rotate(prevState));
				if (values != null)
					values = Rotate(values);
			}
		}

		// Rotates the values in the 4x4 array clock wise
		private int[] Rotate(int[] values)
		{
			int[] newValues = new int[values.Length];
			for (uint y = 0; y < 4; y++)
			{
				for (uint x = 0; x < 4; x++)
				{
					uint newX = 3 - y;
					uint newY = x;
					newValues[newY*4 + newX] = values[y*4 + x];
				}
			}

			return newValues;
		}

		private string PrettyPrint(int[] values)
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

		[Fact]
		public void ShouldMerge()
		{
			Board board = CreateBoard(2, 4, 8, 16,
			                          2, 4, 8, 16);

			AssertMoves(board,
			            4, 8, 16, 32,
			            0, 0, 0, 0,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldMerge2()
		{
			Board board = CreateBoard(2, 4, 8, 16,
			                          0, 0, 0, 0,
			                          2, 4, 8, 16);

			AssertMoves(board,
			            4, 8, 16, 32,
			            0, 0, 0, 0,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldDoubleMerge()
		{
			Board board = CreateBoard(2, 4, 8, 64,
			                          2, 4, 8, 64,
			                          4, 2, 16, 4,
			                          4, 2, 16, 4);

			AssertMoves(board,
			            4, 8, 16, 128,
			            8, 4, 32, 8,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldDoubleMergeEqs()
		{
			Board board = CreateBoard(2, 4, 8, 16,
			                          2, 4, 8, 16,
			                          2, 4, 8, 16,
			                          2, 4, 8, 16);

			AssertMoves(board,
			            4, 8, 16, 32,
			            4, 8, 16, 32,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldMoveToEmpty()
		{
			Board board = CreateBoard(0, 0, 0, 0,
			                          2, 4, 8, 16);

			AssertMoves(board,
			            2, 4, 8, 16,
			            0, 0, 0, 0,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldMoveToEmpty2()
		{
			Board board = CreateBoard(0, 0, 0, 0,
			                          0, 0, 0, 0,
			                          0, 0, 0, 0,
			                          2, 8, 2, 4);

			AssertMoves(board,
			            2, 8, 2, 4,
			            0, 0, 0, 0,
			            0, 0, 0, 0,
			            0, 0, 0, 0);
		}

		[Fact]
		public void ShouldNotMerge()
		{
			Board board = CreateBoard(0, 00, 00, 00,
			                          2, 04, 32, 16,
			                          4, 08, 64, 02,
			                          2, 16, 02, 04);

			AssertMoves(board,
			            2, 04, 32, 16,
			            4, 08, 64, 02,
			            2, 16, 02, 04,
			            0, 00, 00, 00);
		}

		[Fact]
		public void ShouldFail()
		{
			Board board = CreateBoard(02, 04, 08, 016,
			                          04, 08, 16, 032,
			                          08, 16, 32, 064,
			                          16, 32, 64, 128);

			AssertMoves(board, null);
		}
	}
}
