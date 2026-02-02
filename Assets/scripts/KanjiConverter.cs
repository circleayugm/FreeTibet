using System;
using System.Text;
using System.Linq;

public class KanjiConverter
{
	static readonly string[] KanjiDigits = { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

	public static string ToFormattedKanji(long number)
	{
		if (number > 2000000000)
			number = 2000000000;

		if (number == 0)
			return "〇";

		long oku = number / 100000000;
		long man = (number % 100000000) / 10000;
		long lower = number % 10000;

		StringBuilder sb = new StringBuilder();

		// 億部（例：十四億）
		if (oku > 0)
			sb.Append(ConvertTwoDigitKanji((int)oku)).Append("億");

		// 万部（4桁ゼロ埋め）
		if (oku > 0 || man > 0)
			sb.Append(ConvertFixedFourDigitKanji((int)man)).Append("万");

		// 下4桁（ゼロ埋め）
		if (oku > 0 || man > 0 || lower > 0)
			sb.Append(ConvertFixedFourDigitKanji((int)lower));

		return sb.ToString();
	}

	// 2桁以下：自然な漢数字（十四、二十など）
	static string ConvertTwoDigitKanji(int number)
	{
		if (number < 10)
			return KanjiDigits[number];

		int tens = number / 10;
		int ones = number % 10;

		string result = "";
		result += (tens == 1 ? "十" : KanjiDigits[tens] + "十");
		if (ones != 0)
			result += KanjiDigits[ones];

		return result;
	}

	// 必ず4桁の数字を漢数字へ（ゼロも含む）
	static string ConvertFixedFourDigitKanji(int number)
	{
		return number.ToString("D4") // 4桁にゼロ埋め
			.ToCharArray()
			.Aggregate("", (acc, ch) => acc + KanjiDigits[ch - '0']);
	}
}
