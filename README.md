# Kalman-filtering
Prediction of network bandwidth using Kalman filter

预测原理
带宽估算模型
d(i) = dL(i) / c + w(i) 
其中，d(i)两帧数据的网络传输时间差，dL(i)两帧数据的大小差， c为网络传输能力， w(i)是我们关注的重点， 它主要由三个因素决定：发送速率， 网络路由能力， 以及网络传输能力。w(i)符合高斯分布， 有如下结论：当w(i)增加是，占用过多带宽（over-using)；当w(i)减少时，占用较少带宽（under-using)；为0时，用到恰好的带宽。所以，只要我们能计算出w(i)，即能判断目前的网络使用情况，从而增加或减少发送的速率。
可进一步表示成
 d(i) = dL(i) / C(i) + m(i) + v(i) 
 其中，dL(i)两帧数据的大小差， c为网络传输能力，m(i)为网络抖动(可能大于0：比之前的网络延迟增大；也可以小于0：比之前的网络延迟减小)，v(i)为测量误差
 
kalman-filters算法原理：
theta_hat(i) = [1/C_hat(i) m_hat(i)]^T // i时间点的状态由C, m共同表示，theta_hat(i)即此时的估算值
z(i) = d(i) - h_bar(i)^T * theta_hat(i-1) //d(i)为测试值，可以很容易计算出， 后面的可以认为是d(i-1)对d(i)的估算值， 因此z(i)就是d(i)的偏差，即residual
theta_hat(i) = theta_hat(i-1) + z(i) * k_bar(i) //好了，这个就是我们要的结果，关键是k值的选取，下面两个公式即是取k值的。最优估计值
k_bar(i) = (E(i-1) * h_bar(i)) / (var_v_hat + h_bar(i)^T * E(i-1) * h_bar(i))//k_bar(i)为卡尔曼增益，(var_v_hat + h_bar(i)^T * E(i-1) * h_bar(i))即为代码中的denom
E(i) = (I - K_bar(i) * h_bar(i)^T) * E(i-1) + Q(i) // h_bar(i)由帧的数据包大小算出，E(i)为估计误差协方差阵， Q(i)为过程噪声协方差
由此可见，需要设置初始状态、非测量值的变量有E(0)，Q(1)，C_hat(0)，m_hat(0)，
我们只需要知道当前帧的长度，发送时间，接收时间以及前一帧的状态，就可以计算出网络使用情况。
结合卡尔曼滤波的数学公式，可以看出基本匹配。
卡尔曼公式可以自行谷歌百度下。

 
