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
////��������ͬʱ·��ѹ��
//int find(int x)
//{
//	int r = x;
//	while (pre[r] != r)  //ֱ���ҵ�����Ϊֹ����·��ѹ��
//		r = pre[r];
//
//	int i = x; int j;    //·��ѹ��
//	while (i != r)
//	{
//		j = pre[i];       //i���ϼ�  
//		pre[i] = r;       //ָ�������
//		i = j;             //����i
//	}
//
//	return r;
//}
//
////void join(int x, int y)   //������֮����������
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
//	cin >> n >> m;                                                                 //�տ�ʼ��ʱ����n������һ��·��û�� //��ôҪ��n-1��·���ܰ�����������
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
//	for (i = 1;i <= n;i++) { pre[i] = i; }                //����m��·
//	int m1 = m;
//	while (m--)
//	{    //������δ��룬��ʵ����join������ֻ�������Ķ�����Ӧ��ĿҪ��
//		 //ÿ����һ��·�������Ķ˵�p1��p2�Ƿ��Ѿ���һ����ͨ��֧����
//		cin >> p1 >> p2;
//		f1 = find(p1);
//		f2 = find(p2);
//		//����ǲ���ͨ�ģ���ô����������֧������
//		//��֧�������ͼ�����1�����轨��·Ҳ�ͼ���1
//		if (f1 != f2)
//		{
//			pre[f2] = f1;
//		}
//		//��������Ѿ���ͨ�ˣ���ô����·ֻ����ͼ��������һ���� //����ͨ��û���κ�Ӱ�죬���ӵ�
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
//	//��������Ҫ�޵�·����
//	printf("%d\n", num);
//
//	while (1);
//	return 0;
//}