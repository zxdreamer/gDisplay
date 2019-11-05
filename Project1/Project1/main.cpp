#include <iostream>
#include <string>
#include <algorithm>
#include <vector>
#include <map>
#include <sstream>
#include <stack>
#include <set>
#include <deque>
#include <list>

using namespace std;
vector<vector<int>> generateMatrix(int n) {
	int N = n*n;
	vector<vector<int>> matrix(n, vector<int>(n, 0));

	int dr[4] = { 0,1,0,-1 };
	int dc[4] = { 1,0,-1,0 };
	int r = 0, c = 0, di = 0;
	vector<vector<bool>> seen(n+1, vector<bool>(n+1, false));

	for (int i = 1;i <= n*n;i++)
	{
		matrix[r][c] = i;
		int cr = r + dr[di];
		int cc = c + dc[di];		
		seen[r][c] = true;
		if (cr >= 0 && cr<n && cc >= 0 && cc<n && !seen[cr][cc])
		{
			r = cr;
			c = cc;
		}
		else
		{
			di = (di + 1) % 4;
			r += dr[di];
			c += dc[di];
		}
		
		//seen[r][c] = true;
	}

	return matrix;
}

int main()
{
	auto a = generateMatrix(4);
	while (1);
	return 0;
}