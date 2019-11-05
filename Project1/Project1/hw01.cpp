#include <iostream>
#include <vector>
#include <algorithm>
#include <string>
#include <sstream>
#include <map>
using namespace std;

vector<char> MyStrSplit(string s, char c, map<char, int>& mp)
{
	vector<char> vc;
	string sb = s;
	int h = 0;
	int index = sb.find(',');
	while (index != string::npos)
	{
		sb = s.substr(h, index - h);
		mp[sb[0]] = stoi(sb.substr(2));
		vc.push_back(sb[0]);
		h = index + 1;
		index = s.find(',', h);
	}

	sb = s.substr(h, index - h);
	mp[sb[0]] = stoi(sb.substr(2));
	vc.push_back(sb[0]);
	return vc;
}
vector<char> MyStrSplit2(string s, char c, map<char, int>& mp)
{
	vector<char> vc;
	stringstream ss(s);
	string st;
	while (getline(ss, st, ','))
	{
		mp[st[0]] = stoi(st.substr(2));
		vc.push_back(st[0]);
	}
	return vc;
}
int main01()
{
	string s = "a:3,b:5,c:2@a:1,b:2";
	int id = s.find('@');      //≤È’“'@'
	string s1, s2;

	if (id != string::npos)
	{
		s1 = s.substr(0, id);
		s2 = s.substr(id + 1);
		if (s2 == "")
		{
			cout << s1 << endl;
			return 0;
		}
	}
	else                      //’“≤ªµΩ'@'
	{
		cout << s << endl;
		return 0;
	}
	vector<char> vc;
	map<char, int> mp1;
	vc = MyStrSplit2(s1, ',', mp1);
	map<char, int> mp2;
	MyStrSplit2(s2, ',', mp2);
	for (auto m : mp2)
	{
		mp1[m.first] -= m.second;
	}
	for (int i = 0;i<vc.size() - 1;i++)
	{
		if (mp1[vc[i]] != 0)
			cout << vc[i] << ":" << mp1[vc[i]] << ",";
	}
	cout << vc[vc.size() - 1] << ":" << mp1[vc[vc.size() - 1]] << endl;
	while (1);
	return 0;
}