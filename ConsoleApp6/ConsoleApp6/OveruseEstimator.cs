using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{

    public enum BandwidthUsage
    {
        kBwNormal = 0,
        kBwUnderusing = 1,
        kBwOverusing = 2,
    }
    public class OveruseEstimator
    {
        public int kMinFramePeriodHistoryLength = 60;
        public int kDeltaCounterMax = 1000;
        public OverUseDetectorOptions OverUseDetectorOptions;
        private int num_of_deltas_;
        private double slope_;
        private double offset_;
        private double prev_offset_;
        private double[,] E_;
        private double[] process_noise_;
        private double avg_noise_;
        private double var_noise_;
        List<double> ts_delta_hist_;
        public OveruseEstimator(OverUseDetectorOptions overUseDetectorOptions)
        {
            OverUseDetectorOptions = overUseDetectorOptions;
            num_of_deltas_ = 0;
            slope_ = overUseDetectorOptions.initial_slope;
            offset_ = overUseDetectorOptions.initial_offset;
            prev_offset_ = overUseDetectorOptions.initial_offset;

            E_ = new double[2, 2];
            process_noise_ = new double[2];
            avg_noise_ = overUseDetectorOptions.initial_avg_noise;
            var_noise_ = overUseDetectorOptions.initial_var_noise;
            ts_delta_hist_ = new List<double>();
            E_ = overUseDetectorOptions.initial_e;
            process_noise_ = overUseDetectorOptions.initial_process_noise;
        }


        public void Update(long t_delta,
                              double ts_delta,
                              int size_delta,
                              BandwidthUsage current_hypothesis)
        {

            double min_frame_period = UpdateMinFramePeriod(ts_delta);
            double t_ts_delta = t_delta - ts_delta;
            double fs_delta = size_delta;

            ++num_of_deltas_;
            if (num_of_deltas_ > kDeltaCounterMax)
            {
                num_of_deltas_ = kDeltaCounterMax;
            }

            // Update the Kalman filter.
            E_[0, 0] += process_noise_[0];
            E_[1, 1] += process_noise_[1];

            if ((current_hypothesis == BandwidthUsage.kBwOverusing && offset_ < prev_offset_) ||
                (current_hypothesis == BandwidthUsage.kBwUnderusing && offset_ > prev_offset_))
            {
                E_[1, 1] += 10 * process_noise_[1];
            }

            double[] h = new double[2] { fs_delta, 1.0 };
            double[] Eh = new double[2] {E_[0,0]*h[0] + E_[0,1]*h[1],
                        E_[1,0]*h[0] + E_[1,1]*h[1]};

            double residual = t_ts_delta - slope_ * h[0] - offset_;

            bool in_stable_state = (current_hypothesis == BandwidthUsage.kBwNormal);
            double max_residual = 3.0 * Math.Sqrt(var_noise_);
            // We try to filter out very late frames. For instance periodic key
            // frames doesn't fit the Gaussian model well.
            if (Math.Abs(residual) < max_residual)
            {
                UpdateNoiseEstimate(residual, min_frame_period, in_stable_state);
            }
            else
            {
                UpdateNoiseEstimate(residual < 0 ? -max_residual : max_residual,
                                    min_frame_period, in_stable_state);
            }

            double denom = var_noise_ + h[0] * Eh[0] + h[1] * Eh[1];

            double[] K = new double[2] {Eh[0] / denom,
                       Eh[1] / denom};

            double[,] IKh = new double[2, 2]{{1.0 - K[0]*h[0], -K[0]*h[1]},
                            {-K[1]* h[0],1.0 - K[1]* h[1]}};
            double e00 = E_[0, 0];
            double e01 = E_[0, 1];

            // Update state.
            E_[0, 0] = e00 * IKh[0, 0] + E_[1, 0] * IKh[0, 1];
            E_[0, 1] = e01 * IKh[0, 0] + E_[1, 1] * IKh[0, 1];
            E_[1, 0] = e00 * IKh[1, 0] + E_[1, 0] * IKh[1, 1];
            E_[1, 1] = e01 * IKh[1, 0] + E_[1, 1] * IKh[1, 1];

            // The covariance matrix must be positive semi-definite.
            bool positive_semi_definite = E_[0, 0] + E_[1, 1] >= 0 &&
                E_[0, 0] * E_[1, 1] - E_[0, 1] * E_[1, 0] >= 0 && E_[0, 0] >= 0;
            Console.WriteLine(positive_semi_definite);
            if (!positive_semi_definite)
            {

            }

            slope_ = slope_ + K[0] * residual;
            prev_offset_ = offset_;
            offset_ = offset_ + K[1] * residual;
            Console.WriteLine($"slope_:{ (double)1/slope_}offset_: {offset_}");
        }

        public double UpdateMinFramePeriod(double ts_delta)
        {
            double min_frame_period = ts_delta;
            if (ts_delta_hist_.Count() >= kMinFramePeriodHistoryLength)
            {
                //出队第一个
                ts_delta_hist_.Remove(0);
            }
            foreach (var it in ts_delta_hist_)
            {
                if (min_frame_period < it)
                    min_frame_period = it;
            }

            ts_delta_hist_.Add(ts_delta);
            return min_frame_period;
        }

        public void UpdateNoiseEstimate(double residual,
                                           double ts_delta,
                                           bool stable_state)
        {
            if (!stable_state)
            {
                return;
            }
            // Faster filter during startup to faster adapt to the jitter level
            // of the network. |alpha| is tuned for 30 frames per second, but is scaled
            // according to |ts_delta|.
            double alpha = 0.01;
            if (num_of_deltas_ > 10 * 30)
            {
                alpha = 0.002;
            }
            // Only update the noise estimate if we're not over-using. |beta| is a
            // function of alpha and the time delta since the previous update.
            double beta = Math.Pow(1 - alpha, (ts_delta * 30.0 / 1000.0));
            avg_noise_ = beta * avg_noise_
                        + (1 - beta) * residual;
            var_noise_ = beta * var_noise_
                        + (1 - beta) * (avg_noise_ - residual) * (avg_noise_ - residual);
            if (var_noise_ < 1)
            {
                var_noise_ = 1;
            }
        }



    }
}
