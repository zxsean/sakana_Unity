<img src="Assets/Sakana/Resources/chisato.png" height="160px" /><img src="Assets/Sakana/Resources/takina.png" height="160px" />



# 🐟「Sakana!」石蒜模拟器 但是Unity

**[itorr/sakana: 🐟「Sakana!」石蒜模拟器](https://github.com/itorr/sakana)的Unity实现.**

代码几乎直翻原版.

**引用原版说明**

> ## 插画来源
>
> 大伏アオ  
> [@blue00f4](https://twitter.com/blue00f4)  
> [Pixiv](https://pixiv.me/aoiroblue1340)  

## 必要的第三方库

[Unity-UI-Extensions/com.unity.uiextensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions)用于弹簧线段的绘制.

## 视频演示

[🐟「Sakana!」石蒜模拟器 但是Unity](https://www.bilibili.com/video/BV1KP411G7GM/)

## 功能

 - 按住立牌拖拽、松手后立牌会向反方向弹跳
 - 点击底座切换角色

**其余参数部分可以参考原版中的进行调整, 例如衰减惯性等, 均提供了接口.**

## 已知问题

- 没有采用原版的帧率限制, 手感上面会有点问题~ 欢迎有条件的小伙伴pr一下.
- line的实现部分是引用的[Unity-UI-Extensions/com.unity.uiextensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions), 略重了一点.
- 弹簧链接部分和图片在一定角度上会有点问题~

## License

[MIT](https://opensource.org/licenses/MIT)

Copyright (c) 2022, ZeaLotSean