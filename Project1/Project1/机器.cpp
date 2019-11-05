//#include <iostream>
//#include <vector>
//#include <algorithm>
//using namespace std;
//int main()
//{
//	int n, m;
//	cin >> n >> m;
//	vector<vector<int> > mechine(n, { 0,0,0 });
//	vector<vector<int> > asgn(m, { 0,0 });
//	int z, w, x, y;
//	int i = 0;
//	while (i<n)
//	{
//		cin >> z >> w;
//		mechine[i][0] = z;
//		mechine[i][1] = w;
//		i++;
//	}
//	i = 0;
//	while (i<m)
//	{
//		cin >> x >> y;
//		asgn[i][0] = x;
//		asgn[i][1] = y;
//		i++;
//	}
//	sort(mechine.begin(), mechine.end(), [](vector<int> a, vector<int> b)->bool {if (a[0] == b[0]) { a[1] > b[1]; }return a[0] < b[0];});
//	sort(asgn.begin(), asgn.end(), [](vector<int> a, vector<int> b)->bool {if (a[0] == b[0]) { a[1] > b[1]; }return a[0] < b[0];});
//	int fi = 0;
//	long long val = 0;
//	int cnt = 0;
//	for (int i = 0;i<asgn.size();i++)
//	{
//		for (int j = 0;j<mechine.size();j++)
//		{
//			if (asgn[i][0] <= mechine[j][0])  //任务时间小于机器时间
//			{
//				if (asgn[i][1] <= mechine[j][1])  //任务等级小于机器等级
//				{
//					if (mechine[j][2] == 0)        //机器未服务
//					{
//						mechine[j][2] = 200 * asgn[i][0] + 3 * asgn[i][1];  //存服务的价值
//						cnt++;
//					}
//					else
//					{
//						int tmp = j;
//						j++;
//						while (j < mechine.size() - 1 && mechine[j][2] != 0)
//						{
//							j++;
//						}
//
//						if (j < mechine.size() - 1)  //后面有机器
//						{
//							mechine[j][2] = 200 * asgn[i][0] + 3 * asgn[i][1];  //存服务的价值
//							cnt++;
//						}
//						else
//						{
//							mechine[tmp][2] = mechine[tmp][2] > 200 * asgn[i][0] + 3 * asgn[i][1] ? mechine[tmp][2] : 200 * asgn[i][0] + 3 * asgn[i][1];
//						}
//					}
//					break;
//				}
//			}
//		}
//	}
//	for (auto v : mechine)
//	{
//		val = val + v[2];
//	}
//	cout << cnt << " " << val << endl;
//	while (1);
//	return 0;
//}