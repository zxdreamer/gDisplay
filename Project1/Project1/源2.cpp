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
//int getMax(int a, int b)
//{
//	int min = a < b ? a : b;
//	for (int i = min;i >= i;i--)
//	{
//		if (a%i == 0 && b%i == 0)
//			return i;
//	}
//	return 1;
//}
//
//int main()
//{
//	int n;
//	cin >> n;
//	vector<int> arr(n, 0);
//	for (int i = 0;i < n;i++)
//		cin >> arr[i];
//	stack<int> sk;
//	int fenzi = 0;
//	int fenmu = 0;
//	int num1 = 0;
//	int num2 = 0;
//	if (arr.size() == 1)
//		cout << arr[0] + 1 << endl;
//	else
//	{
//		for (int i = 0;i < arr.size();i++)
//		{
//			sk.push(arr[i]);
//		}
//		if (sk.size() > 0)
//		{
//			num1 = sk.top();
//			sk.pop();
//			fenmu = num1;
//		}
//		if (sk.size() > 0)
//		{
//			num2 = sk.top();
//			sk.pop();
//			fenzi = num1*num2 + 1;
//		}
//		while (sk.size() > 0)
//		{
//			int tmp = sk.top();
//			sk.pop();
//			int tmp2 = fenmu;
//			fenmu = fenzi;
//			fenzi = fenzi*tmp + tmp2;
//		}
//		int maxNum = getMax(fenzi, fenmu);
//		cout << fenzi / maxNum << " " << fenmu / maxNum;
//	}
//
//	while (1);
//	return 0;
//}