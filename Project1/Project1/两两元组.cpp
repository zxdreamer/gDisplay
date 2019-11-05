//#include <iostream>
//#include <map>
//#include<algorithm>
//using namespace std;
//int main06()
//{
//	int n;
//	cin >> n;
//	map<int, int> mp;
//	int a;
//	while (n>0)
//	{
//		cin >> a;
//		auto res = mp.insert({ a,1 });
//		if (res.second == false)
//		{
//			mp[a]++;
//		}
//		n--;
//	}
//	//	sort(mp.begin(), mp.end());
//	int rpcnt = 0;
//	for (auto m : mp)
//	{
//		if (m.second > 1)
//		{
//			int len = m.second;
//			rpcnt += (len - 1 + 1)*(len - 1) / 2;
//		}
//	}
//	auto end = mp.end();
//	end--;
//	int mx = mp.begin()->second * end->second;
//	if (rpcnt != 0)
//	{
//		cout << rpcnt << " " << mx << endl;
//	}
//	else
//	{
//		int mi = INT_MAX;
//		int pre = 0, nxt = 0;
//		int index = 0;
//		auto end = mp.end()--;
//		for (auto beg = mp.begin();beg != end;)
//		{
//			if ((++beg)->first - beg->first < mi)
//			{
//				pre = index;
//				mi = (beg++)->first - beg->first;
//			}
//			index++;
//		}
//		int micnt = mp[pre] * mp[pre + 1];
//		cout << micnt << " " << mx << endl;
//	}
//
//	while (1);
//	return 0;
//}