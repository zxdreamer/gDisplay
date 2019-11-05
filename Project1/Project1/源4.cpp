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
//
//int getMaxScore(vector<int> arr, int res, int len)
//{
//	if (len == 1)
//		return arr[0] + res;
//	int min = INT_MAX;
//	int index = 0;
//	for (int i = 0;i < arr.size();i++)
//	{
//		if (arr[i] > 0)
//		{
//			min = arr[i] < min ? arr[i] : min;
//			index = arr[i] == min ? i : index;
//		}
//	}
//
//	int tmp = index - 1;
//	int left = 0, right = 0;
//	while (tmp >= 0)
//	{
//		if (arr[tmp] != -1)
//		{
//			left = arr[tmp];
//			break;
//		}
//		tmp--;
//	}
//	tmp = index + 1;
//	while (tmp < arr.size())
//	{
//		if (arr[tmp] != -1)
//		{
//			right = arr[tmp];
//			break;
//		}
//		tmp++;
//	}
//	left = left == 0 ? 1 : left;
//	right = right == 0 ? 1 : right;
//	res += left*right*arr[index];
//	arr[index] = -1;
//	len--;
//	return getMaxScore(arr, res, len);
//}
//int main()
//{
//	int n;
//	cin >> n;
//	vector<int> arr(n, 0);
//	for (int i = 0;i < n;i++)
//		cin >> arr[i];
//
//	cout << getMaxScore(arr, 0, arr.size()) << endl;
//	while (1);
//	return 0;
//}