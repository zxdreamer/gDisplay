#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int dx[2] = { 1,1 };
int dy[2] = { 1,-1 };

void paNext(int x, int y, vector<vector<char>>& paint, int flag)
{
	int xx = x + dx[flag];  //ºá
	int yy = y + dy[flag];  //Êú
	while (xx<paint.size() && xx >= 0 && yy<paint[0].size() && yy >= 0)
	{
		if (paint[xx][yy] == 'X')break;
		if (flag == 0)
		{
			if (paint[xx][yy] == 'Y')
				paint[xx][yy] = 'X';
			else if (paint[xx][yy] == 'G')
				paint[xx][yy] = 'B';
			else break;
		}
		if (flag == 1)
		{
			if (paint[xx][yy] == 'B')
				paint[xx][yy] = 'X';
			else if (paint[xx][yy] == 'G')
				paint[xx][yy] = 'Y';
			else break;
		}
		xx += dx[flag], yy += dy[flag];
	}
}
int main02()
{
	int n, m;
	cin >> n >> m;
	char c;
	vector<vector<char>> paint(n, vector<char>(m, '0'));
	for (int i = 0;i < n;i++)
	{
		for (int j = 0;j < m;j++)
		{
			cin >> c;
			paint[i][j] = c;
		}
	}

	int cnt = 0;
	for (int i = 0;i < n;i++)
	{
		for (int j = 0;j < m;j++)
		{
			if (paint[i][j] != 'X')
			{
				if (paint[i][j] == 'Y')
				{
					paNext(i, j, paint, 0);
					cnt++;
				}
				else if (paint[i][j] == 'B')
				{
					paNext(i, j, paint, 1);
					cnt++;
				}
				if (paint[i][j] == 'G')
				{
					paNext(i, j, paint, 0);
					paNext(i, j, paint, 1);
					cnt += 2;
				}
			}
			paint[i][j] = 'X';
		}
	}
	cout << cnt << endl;
	while (1);
	return 0;
}