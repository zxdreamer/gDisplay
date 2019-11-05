//#include <iostream>
//#include <vector>
//#include <algorithm>
//#include <string>
//#include <map>
//using namespace std;
//
//int main()
//{
//	int n;
//	cin >> n;
//	vector<long long> vc;
//	long long a;
//	while (n > 0)
//	{
//		n--;
//		cin >> a;
//		vc.push_back(a);
//	}
//	vector<int> vs(vc.begin(), vc.end());
//	sort(vs.begin(), vs.end());
//	int index = 0;
//	int cnt = 0;
//	for (int i = 0;i < vc.size();i = index)
//	{
//		int j;
//		for (j = index;j < vs.size();j++)
//		{
//			if (vc[i] == vs[j])
//			{
//				cnt++;
//				index = j + 1;
//				break;
//			}
//		}
//		if (index >= vs.size())
//			break;
//	}
//
//	cout << cnt << endl;
//	while (1);
//	return 0;
//}


//#include <iostream>
//#include <vector>
//#include <algorithm>
//#include <string>
//#include <map>
//using namespace std;
//
//int main()
//{
//	long long n;
//	cin >> n;
//	vector<long long> v(n, 0);
//
//	for (int i = 0;i < n;i++)
//	{
//		cin >> v[i];
//	}
//	long long cont = 0;
//	for (long long i = 0;i < n;++i)
//	{
//		long long j;
//		for (j = n - 1;j > i;--j)
//		{
//			if (v[j] < v[i])
//			{
//				break;
//			}
//		}
//		i = j;
//		++cont;
//	}
//	cout << cont << endl;
//
//	return 0;
//}

	//#include <iostream>
	//#include <vector>
	//#include <algorithm>
	//#include <string>
	//#include <map>
	//using namespace std;
	//
	//long long dp[50][50];
	//int main()
	//{
	//	int n;
	//	cin >> n;
	//	int k = n / 2;
	//	for (int i = 0;i <= n;i++)
	//		dp[i][0] = 1;
	//
	//	for (int i = 1;i <= n;i++)
	//	{
	//		for (int j = 0;j <= k;j++)
	//		{
	//			dp[i][j] = dp[i - 1][j];
	//			for (int p = 1;p <= i - 1;p++)
	//			{
	//				for (int q = 0;q <= j - 1;q++)
	//				{
	//					dp[i][j] += dp[p - 1][q] * dp[i - 1 - p][j - 1 - q];
	//				}
	//			}
	//		}
	//	}
	//	cout << dp[n][k] << endl;
	//	return 0;
	//}


//#include <iostream>
//#include <vector>
//#include <algorithm>
//#include <string>
//#include <map>
//using namespace std;
//
//int main()
//{
//	int n;
//	cin >> n;
//	vector<vector<int>> vc(4, vector<int>(4, 0));
//
//	int t = 0;
//	while (t < 4)
//	{
//		for (int i = 0;i < 4;i++)
//			cin >> vc[t][i];
//		t++;
//	}
//
//
//	for (int i = 0;i < 4;i++)
//	{
//		for (int j = 0;j < 4;j++)
//		{
//			cout << vc[i][j] << " ";
//		}
//		cout << endl;
//	}
//	cout << endl;
//
//	while (1);
//	return 0;
//}



//#include <iostream>
//#include <vector>
//#include <algorithm>
//#include <string>
//#include <map>
//using namespace std;
//
//int main()
//{
//	int n;
//	cin >> n;
//	n = n / 2;
//	vector<int> x{ 1,1,2 };
//
//	if (n < 3)
//		cout << x[n] << endl;
//	else
//	{
//		for (int k = 3;k < n + 1;k++)
//		{
//			int res = 0;
//			if (k % 2 == 0)
//			{
//				for (int i = 0;i < k / 2;i++)
//				{
//					res += x[i] * x[-i - 1];
//				}
//				x.push_back(2 * res);
//			}
//			else
//			{
//				for (int i = 0;i < k / 2;i++)
//				{
//					res += x[i] * x[-i - 1];
//				}
//				x.push_back(2 * res + x[k / 2] * x[k / 2]);
//			}
//		}
//		cout << x[n] % 1000000007 << endl;
//	}
//
//	while (1);
//	return 0;
//}