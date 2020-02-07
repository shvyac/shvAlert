using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shvAlert
{
    ///[Serializable]
    public class UDPModel
    {
        /* public class HeartbeatModel
        {
            //public HeartbeatModel() { }
            public string heartbeat_client_id { get; set; }
            public UInt32 heartbeat_maximum_schema_number { get; set; }
            public string heartbeat_version { get; set; }
            public string heartbeat_revision { get; set; }
        }*/

        public class StatusModel
        {
            public string status_client_id { get; set; } //Text
            public string status_dial_frequency { get; set; } //Word64
            public string status_mode { get; set; } //Text
            public string status_dx_call { get; set; } //Text
            public string status_report { get; set; } //Text
            public string status_tx_mode { get; set; } //Text
            public string status_tx_enabled { get; set; } //Bool
            public string status_transmitting { get; set; } //Bool
            public string status_decoding { get; set; } //Bool
            public string status_rx_df { get; set; } //Word32
            public string status_tx_df { get; set; } //Word32
            public string status_de_call { get; set; } //Text
            public string status_de_grid { get; set; } //Text
            public string status_dx_grid { get; set; } //Text
            public string status_tx_watchdog { get; set; } //Bool
            public string status_submode { get; set; } //Text
            public string status_fast_mode { get; set; } //Bool
        }

        public class DecodeModel
        {
            public string decode_client_id { get; set; }   // Text
            public bool decode_new { get; set; }   // Bool
            public DateTime decode_time { get; set; }   // DiffTime
            public Int32 decode_snr { get; set; }   // Int
            public Double decode_delta_time { get; set; }   // Double
            public UInt32 decode_delta_frequency { get; set; }   // Word32
            public string decode_mode { get; set; }   // Text
            public string decode_message { get; set; }  // Text
            //
            public string alloc_left { get; set; }  //CALLSIGN or CQ
            public string alloc_right { get; set; } //CALLSIGN
        }
    }
}

/*
data Heartbeat = Heartbeat {
    heartbeat_client_id :: Text
  , heartbeat_maximum_schema_number :: Word32
  , heartbeat_version      :: Text
  , heartbeat_revision     :: Text
  } deriving (Read, Show, Eq, Generic)

  data Status = Status {
    status_client_id :: Text
  , status_dial_frequency :: Word64
  , status_mode :: Text
  , status_dx_call :: Text
  , status_report :: Text
  , status_tx_mode :: Text
  , status_tx_enabled :: Bool
  , status_transmitting :: Bool
  , status_decoding :: Bool
  , status_rx_df :: Word32
  , status_tx_df :: Word32
  , status_de_call :: Text
  , status_de_grid :: Text
  , status_dx_grid :: Text
  , status_tx_watchdog :: Bool
  , status_submode :: Text
  , status_fast_mode :: Bool
} deriving (Read, Show, Eq, Generic)

data Decode = Decode {
    decode_client_id :: Text
  , decode_new :: Bool
  , decode_time :: DiffTime
  , decode_snr  :: Int
  , decode_delta_time :: Double
  , decode_delta_frequency :: Word32
  , decode_mode :: Text
  , decode_message :: Text
  } deriving (Read, Show, Eq, Generic)

  data Logged = Logged {
    logged_client_id :: Text
  , logged_date_time_off :: Word64
  , logged_dx_call :: Text
  , logged_dx_grid :: Text
  , logged_dial_frequency :: Word64
  , logged_mode :: Text
  , logged_report_send :: Text
  , logged_report_received :: Text
  , logged_tx_power :: Text
  , logged_comments :: Text
  , logged_name :: Text
  , logged_date_time_on :: Word64
  } deriving (Read, Show, Eq, Generic)

*/