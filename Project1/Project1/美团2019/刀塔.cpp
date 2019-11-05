#include <iostream>
#include <vector>
#include <string>
#include <map>
using namespace std;
int main()
{
	int T;
	cin >> T;
	int s;	
	int n, d, x, y;
	int t0, t1, t2;
	while (T > 0)
	{
		T--;
		cin >> s;
		cin >> n >> d >> x >> y;
		cin >> t0 >> t1 >> t2;
		int i = 1;
		int xb = 0, tj = 0, pj = 0;
		if ((s = s - n*d) <= 0)
		{
			cout << "NO" << endl;
			continue;
		}
		if ((s = s - x) <= 0)
		{
			cout << "YES" << endl;
			continue;
		}
		if ((s = s - y) <= 0)
		{
			cout << "YES" << endl;
			continue;
		}
		while (s > 0)
		{
			if (i - xb == xb + t0 + 1)
			{
				if ((s = s - n*d) <= 0)
				{
					cout << "NO" << endl;
					break;
				}
				xb = i;
			}
			if (i - tj == tj+t1 + 1)
			{
				if ((s = s - x) <= 0)
				{
					cout << "YES" << endl;
					break;
				}
				tj = i;
			}
			if (i - pj == pj+t2 + 1)
			{
				if ((s = s - y) <= 0)
				{
					cout << "YES" << endl;
					break;
				}
				pj = i;
			}
			i++;
		 }
	}
	while (1);
	return 0;
}