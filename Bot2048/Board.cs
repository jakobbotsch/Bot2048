using System;
using System.Linq;

namespace Bot2048
{
	public class Board
	{
		private readonly int[] _state;

		public Board()
		{
			_state = new int[16];
		}

		public Board(int[] state)
		{
			if (state == null)
				throw new ArgumentNullException(nameof(state));

			if (state.Length != 4*4)
				throw new ArgumentException("Board must have length 4 * 4");

			_state = state;
		}

		public void SetFromBoard(Board board)
		{
			board._state.CopyTo(_state, 0);
		}

		public int[] GetState()
		{
			return (int[])_state.Clone();
		}

		public void Set(uint x, uint y, int value)
		{
			_state[y*4 + x] = value;
		}

		public int Get(uint x, uint y)
		{
			return _state[y*4 + x];
		}

		public void SpawnRandom(Random rand)
		{
			int value = rand.NextDouble() < 0.9 ? 2 : 4;
			while (true)
			{
				int index = rand.Next(0, _state.Length);
				if (_state[index] != 0)
					continue;

				_state[index] = value;
				return;
			}
		}

		public bool Move(Direction dir)
		{
			if (dir == Direction.Up || dir == Direction.Down)
				return MoveVertical(dir == Direction.Up ? 1 : -1);

			return MoveHorizontal(dir == Direction.Left ? 1 : -1);
		}

		private bool MoveVertical(int dy)
		{
			int yStart = dy > 0 ? 0 : 3;
			int yEnd = dy > 0 ? 4 : -1;

			bool result = false;
			for (uint x = 0; x < 4; x++)
			{
				int valPosStart = dy > 0 ? -1 : 4;
				int valPos = valPosStart;
				bool valMerged = false;
				for (int y = yStart; y != yEnd; y += dy)
				{
					int val = Get(x, (uint)y);
					if (val == 0)
						continue;

					// Merge if possible
					if (valPos != valPosStart && Get(x, (uint)valPos) == val && !valMerged)
					{
						Set(x, (uint)valPos, val*2);
						Set(x, (uint)y, 0);
						valMerged = true;
						result = true;
						continue;
					}

					valPos += dy;
					valMerged = false;
					if ((uint)valPos != y)
					{
						Set(x, (uint)valPos, val);
						Set(x, (uint)y, 0);
						result = true;
					}
				}
			}

			return result;
		}

		private bool MoveHorizontal(int dx)
		{
			int xStart = dx > 0 ? 0 : 3;
			int xEnd = dx > 0 ? 4 : -1;

			bool result = false;
			for (uint y = 0; y < 4; y++)
			{
				int valPosStart = dx > 0 ? -1 : 4;
				int valPos = valPosStart;
				bool valMerged = false;
				for (int x = xStart; x != xEnd; x += dx)
				{
					int val = Get((uint)x, y);
					if (val == 0)
						continue;

					// Merge if possible
					if (valPos != valPosStart && Get((uint)valPos, y) == val && !valMerged)
					{
						Set((uint)valPos, y, val*2);
						Set((uint)x, y, 0);
						valMerged = true;
						result = true;
						continue;
					}

					valPos += dx;
					valMerged = false;
					if ((uint)valPos != x)
					{
						Set((uint)valPos, y, val);
						Set((uint)x, y, 0);
						result = true;
					}
				}
			}

			return result;
		}

		public Board Clone()
		{
			Board board = new Board();
			_state.CopyTo(board._state, 0);
			return board;
		}
	}

	public enum Direction
	{
		Up,
		Right,
		Down,
		Left,
	}
}