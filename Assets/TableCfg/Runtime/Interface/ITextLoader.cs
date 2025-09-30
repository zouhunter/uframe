/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表控制器使用接口                                                                *
*//************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.TableCfg
{
    public interface ITextLoader
    {
        System.Text.Encoding Encoding { get; }
        void LoadStream(string path, System.Action<byte, StreamContent> onLoad);
    }
}

/*
 37	IBM037	IBM EBCDIC（美国 - 加拿大）	
437	IBM437	OEM 美国	
500	IBM500	IBM EBCDIC（国际）	
708	ASMO-708	阿拉伯字符 (ASMO 708)	
720	DOS-720	阿拉伯字符 (DOS)	
737	ibm737	希腊字符 (DOS)	
775	ibm775	波罗的海字符 (DOS)	
850	ibm850	西欧字符 (DOS)	
852	ibm852	中欧字符 (DOS)	
855	IBM855	OEM 西里尔语	
857	ibm857	土耳其字符 (DOS)	
858	IBM00858	OEM 多语言拉丁语 I	
860	IBM860	葡萄牙语 (DOS)	
861	ibm861	冰岛语 (DOS)	
862	DOS-862	希伯来字符 (DOS)	
863	IBM863	加拿大法语 (DOS)	
864	IBM864	阿拉伯字符 (864)	
865	IBM865	北欧字符 (DOS)	
866	cp866	西里尔字符 (DOS)	
869	ibm869	现代希腊字符 (DOS)	
870	IBM870	IBM EBCDIC（多语言拉丁语 2）	
874	windows-874	泰语 (Windows)	
875	cp875	IBM EBCDIC（现代希腊语）	
932	shift_jis	日语 (Shift-JIS)	
936	gb2312	简体中文 (GB2312)	*
949	ks_c_5601-1987	朝鲜语	
950	big5	繁体中文 (Big5)	
1026	IBM1026	IBM EBCDIC（土耳其拉丁语 5）	
1047	IBM01047	IBM 拉丁语 1	
1140	IBM01140	IBM EBCDIC（美国 - 加拿大 - 欧洲）	
1141	IBM01141	IBM EBCDIC（德国 - 欧洲）	
1142	IBM01142	IBM EBCDIC（丹麦 - 挪威 - 欧洲）	
1143	IBM01143	IBM EBCDIC（芬兰 - 瑞典 - 欧洲）	
1144	IBM01144	IBM EBCDIC（意大利 - 欧洲）	
1145	IBM01145	IBM EBCDIC（西班牙 - 欧洲）	
1146	IBM01146	IBM EBCDIC（英国 - 欧洲）	
1147	IBM01147	IBM EBCDIC（法国 - 欧洲）	
1148	IBM01148	IBM EBCDIC（国际 - 欧洲）	
1149	IBM01149	IBM EBCDIC（冰岛语 - 欧洲）	
1200	utf-16	Unicode	*
1201	UnicodeFFFE	Unicode (Big-Endian)	*
1250	windows-1250	中欧字符 (Windows)	
1251	windows-1251	西里尔字符 (Windows)	
1252	Windows-1252	西欧字符 (Windows)	*
1253	windows-1253	希腊字符 (Windows)	
1254	windows-1254	土耳其字符 (Windows)	
1255	windows-1255	希伯来字符 (Windows)	
1256	windows-1256	阿拉伯字符 (Windows)	
1257	windows-1257	波罗的海字符 (Windows)	
1258	windows-1258	越南字符 (Windows)	
1361	Johab	朝鲜语 (Johab)	
10000	macintosh	西欧字符 (Mac)	
10001	x-mac-japanese	日语 (Mac)	
10002	x-mac-chinesetrad	繁体中文 (Mac)	
10003	x-mac-korean	朝鲜语 (Mac)	*
10004	x-mac-arabic	阿拉伯字符 (Mac)	
10005	x-mac-hebrew	希伯来字符 (Mac)	
10006	x-mac-greek	希腊字符 (Mac)	
10007	x-mac-cyrillic	西里尔字符 (Mac)	
10008	x-mac-chinesesimp	简体中文 (Mac)	*
10010	x-mac-romanian	罗马尼亚语 (Mac)	
10017	x-mac-ukrainian	乌克兰语 (Mac)	
10021	x-mac-thai	泰语 (Mac)	
10029	x-mac-ce	中欧字符 (Mac)	
10079	x-mac-icelandic	冰岛语 (Mac)	
10081	x-mac-turkish	土耳其字符 (Mac)	
10082	x-mac-croatian	克罗地亚语 (Mac)	
20000	x-Chinese-CNS	繁体中文 (CNS)	
20001	x-cp20001	TCA 台湾	
20002	x-Chinese-Eten	繁体中文 (Eten)	
20003	x-cp20003	IBM5550 台湾	
20004	x-cp20004	TeleText 台湾	
20005	x-cp20005	Wang 台湾	
20105	x-IA5	西欧字符 (IA5)	
20106	x-IA5-German	德语 (IA5)	
20107	x-IA5-Swedish	瑞典语 (IA5)	
20108	x-IA5-Norwegian	挪威语 (IA5)	
20127	us-ascii	US-ASCII	*
20261	x-cp20261	T.61	
20269	x-cp20269	ISO-6937	
20273	IBM273	IBM EBCDIC（德国）	
20277	IBM277	IBM EBCDIC（丹麦 - 挪威）	
20278	IBM278	IBM EBCDIC（芬兰 - 瑞典）	
20280	IBM280	IBM EBCDIC（意大利）	
20284	IBM284	IBM EBCDIC（西班牙）	
20285	IBM285	IBM EBCDIC（英国）	
20290	IBM290	IBM EBCDIC（日语片假名）	
20297	IBM297	IBM EBCDIC（法国）	
20420	IBM420	IBM EBCDIC（阿拉伯语）	
20423	IBM423	IBM EBCDIC（希腊语）	
20424	IBM424	IBM EBCDIC（希伯来语）	
20833	x-EBCDIC-KoreanExtended	IBM EBCDIC（朝鲜语扩展）	
20838	IBM-Thai	IBM EBCDIC（泰语）	
20866	koi8-r	西里尔字符 (KOI8-R)	
20871	IBM871	IBM EBCDIC（冰岛语）	
20880	IBM880	IBM EBCDIC（西里尔俄语）	
20905	IBM905	IBM EBCDIC（土耳其语）	
20924	IBM00924	IBM 拉丁语 1	
20932	EUC-JP	日语（JIS 0208-1990 和 0212-1990）	
20936	x-cp20936	简体中文 (GB2312-80)	*
20949	x-cp20949	朝鲜语 Wansung	*
21025	cp1025	IBM EBCDIC（西里尔塞尔维亚 - 保加利亚语）	
21866	koi8-u	西里尔字符 (KOI8-U)	
28591	iso-8859-1	西欧字符 (ISO)	*
28592	iso-8859-2	中欧字符 (ISO)	
28593	iso-8859-3	拉丁语 3 (ISO)	
28594	iso-8859-4	波罗的海字符 (ISO)	
28595	iso-8859-5	西里尔字符 (ISO)	
28596	iso-8859-6	阿拉伯字符 (ISO)	
28597	iso-8859-7	希腊字符 (ISO)	
28598	iso-8859-8	希伯来字符 (ISO-Visual)	*
28599	iso-8859-9	土耳其字符 (ISO)	
28603	iso-8859-13	爱沙尼亚语 (ISO)	
28605	iso-8859-15	拉丁语 9 (ISO)	
29001	x-Europa	欧罗巴	
38598	iso-8859-8-i	希伯来字符 (ISO-Logical)	*
50220	iso-2022-jp	日语 (JIS)	*
50221	csISO2022JP	日语（JIS- 允许 1 字节假名）	*
50222	iso-2022-jp	日语（JIS- 允许 1 字节假名 - SO/SI）	*
50225	iso-2022-kr	朝鲜语 (ISO)	*
50227	x-cp50227	简体中文 (ISO-2022)	*
51932	euc-jp	日语 (EUC)	*
51936	EUC-CN	简体中文 (EUC)	*
51949	euc-kr	朝鲜语 (EUC)	*
52936	hz-gb-2312	简体中文 (HZ)	*
54936	GB18030	简体中文 (GB18030)	*
57002	x-iscii-de	ISCII 梵文	*
57003	x-iscii-be	ISCII 孟加拉语	*
57004	x-iscii-ta	ISCII 泰米尔语	*
57005	x-iscii-te	ISCII 泰卢固语	*
57006	x-iscii-as	ISCII 阿萨姆语	*
57007	x-iscii-or	ISCII 奥里雅语	*
57008	x-iscii-ka	ISCII 卡纳达语	*
57009	x-iscii-ma	ISCII 马拉雅拉姆语	*
57010	x-iscii-gu	ISCII 古吉拉特语	*
57011	x-iscii-pa	ISCII 旁遮普语	*
65000	utf-7	Unicode (UTF-7)	*
65001	utf-8	Unicode (UTF-8)	*
65005	utf-32	Unicode (UTF-32)	*
65006	utf-32BE	Unicode (UTF-32 Big-Endian)	
 */