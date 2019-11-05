//#include <iostream>
//#include <string>
//#include <vector>
//#include <algorithm>
//using namespace std;
//
//int main()
//{
//	int n;
//	cin >> n;  //1
//	while (n > 0)
//	{
//		n--;
//		int r;
//		cin >> r; //8
//		vector<vector<int>> vv;
//
//		while (r>0)
//		{
//			int len;
//			cin >> len;
//			vector<int> tmp(len * 2);
//			for (int i = 0;i < len * 2;i += 2)
//			{
//				cin >> tmp[i] >> tmp[i + 1];
//			}
//			vv.push_back(tmp);
//			r--;
//		}
//
//		int mx = 0;
//		int cnt = 1;
//		int iscom = 0;
//		for (int j = 0;j < vv.size() - 1;j++)
//		{
//			for (int k = 0;k < vv[j].size();k += 2)
//			{
//				for (int m = j + 1;m < vv.size();m++)
//				{
//					for (int h = 0;h < vv[m].size();h += 2)
//					{
//						if (vv[j][k] == vv[m][h] && vv[j][k + 1] == vv[m][h + 1])
//						{
//							iscom = 1;
//						}
//					}
//					if (iscom == 1)
//						cnt++;
//					else
//					{
//						mx = max(cnt, mx);  //下一层开始找到的最大的连续的个数
//						cnt = 1;
//					}
//					iscom = 0;
//				}
//				mx = max(cnt, mx);
//				cnt = 1;
//				iscom = 0;
//			}
//		}
//		mx = max(mx, cnt);
//		cout << mx << endl;
//	}
//	while (1);
//	return 0;
//}


//#include <iostream>
//#include <vector>
//#include <algorithm>
//#include<numeric>
//#include<list>
//#include<string>
//#include<sstream>
//#include<stack>
//using namespace std;
//int main()
//{
//	int n;
//	cin >> n;
//	vector<int> v;
//	vector<char> vf;
//	string s;
//	cin >> s;
//	string res;
//	for (int i = 0;i < s.size();)
//	{
//		string tmp;
//		while (s[i] >= '0' && s[i] <= '9')
//		{
//			tmp += s[i];
//			i++;
//		}
//		v.push_back(stoi(tmp));
//		tmp = "";
//		if (s[i] == '-')
//		{
//			if (i == 0 || (i - 1 > 0 && s[i - 1] == '+' || s[i - 1] == '-' || s[i - 1] == '*' || s[i - 1] == '/'))
//			{
//				i++;
//				while (s[i] >= '0' && s[i] <= '9')
//				{
//					tmp += s[i];
//					i++;
//				}
//				v.push_back(stoi(tmp));
//			}
//		}
//		if (s[i] == '*' || s[i] == '-' || s[i] == '*' || s[i] == '/')
//		{
//			vf.push_back(s[i]);
//			i++;
//		}
//
//		for (int i = 0;i < vf.size();i++)
//		{
//			if ()
//		}
//	}
//
//	return 0;
//}