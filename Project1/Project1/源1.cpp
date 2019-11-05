//#include <iostream>
//#include <string>
//#include <algorithm>
//#include <vector>
//#include <map>
//#include <sstream>
//#include <stack>
//#include <set>
//#include <deque>
//#include <list>
//
//using namespace std;
//int pre[1000];
////查找掌门同时路径压缩
//int find(int x)
//{
//	int r = x;
//	while (pre[r] != r)  //直到找到掌门为止，无路径压缩
//		r = pre[r];
//
//	int i = x; int j;    //路径压缩
//	while (i != r)
//	{
//		j = pre[i];       //i的上级  
//		pre[i] = r;       //指向大掌门
//		i = j;             //记下i
//	}
//
//	return r;
//}
//
////void join(int x, int y)   //掌门人之间连接起来
////{
////	int fx = find(x);
////	int fy = find(y);
////	if (fx != fy)
////	{
////		pre[fx] = fy;
////	}
////}
//
//int main()
//{
//	int n, k, m, p1, p2, i, total, f1, f2;
//	cin >> n >> m;                                                                 //刚开始的时候，有n个城镇，一条路都没有 //那么要修n-1条路才能把它们连起来
//	vector<int> pnum(n + 1, 0);
//	for (int i = 1;i < n + 1;i++)
//		cin >> pnum[i];
//	int num = 0;
//	if (m == 0)
//	{
//		for (int i = 1;i<n + 1;i++)
//			num = max(num, pnum[i]);
//		cout << num << endl;
//		return 0;
//	}
//	for (i = 1;i <= n;i++) { pre[i] = i; }                //共有m条路
//	int m1 = m;
//	while (m--)
//	{    //下面这段代码，其实就是join函数，只是稍作改动以适应题目要求
//		 //每读入一条路，看它的端点p1，p2是否已经在一个连通分支里了
//		cin >> p1 >> p2;
//		f1 = find(p1);
//		f2 = find(p2);
//		//如果是不连通的，那么把这两个分支连起来
//		//分支的总数就减少了1，还需建的路也就减了1
//		if (f1 != f2)
//		{
//			pre[f2] = f1;
//		}
//		//如果两点已经连通了，那么这条路只是在图上增加了一个环 //对连通性没有任何影响，无视掉
//	}
//	map<int, int> mp;
//	for (int i = 1;i <= n;i++)
//	{
//		auto res = mp.insert({ pre[i],pnum[i] });
//		if (res.second == false)
//			mp[pre[i]] += pnum[i];
//	}
//
//	for (int j = 1;j < mp.size() + 1;j++)
//	{
//		num = max(num, mp[j]);
//	}
//
//	//最后输出还要修的路条数
//	printf("%d\n", num);
//
//	while (1);
//	return 0;
//}