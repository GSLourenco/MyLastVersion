using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using MvcApplication2.GCM;

namespace MvcApplication2.GCM
{
    public class Message
    {
        /* Dont need data because we are not sending playload */
        private readonly String collapseKey;
        private readonly Boolean? delayWhileIdle;
        private readonly int? timeToLive;
        private readonly Boolean? dryRun;
        private readonly String restrictedPackageName;
        private readonly Dictionary<string, string> data;

        public class Builder
        {
            // optional parameters
            public String _collapseKey;
            public Boolean? _delayWhileIdle;
            public int? _timeToLive;
            public Boolean? _dryRun;
            public String _restrictedPackageName;
            public Dictionary<string, string> _data;


            public Builder()
            {
                this._data = new Dictionary<string, string>();
            }
            /**
         * Sets the collapseKey property.
         */
            public Builder collapseKey(String value)
            {
                _collapseKey = value;
                return this;
            }

            /**
             * Sets the delayWhileIdle property (default value is {@literal false}).
             */
            public Builder delayWhileIdle(bool value)
            {
                _delayWhileIdle = value;
                return this;
            }

            /**
        * Sets the time to live, in seconds.
        */
            public Builder timeToLive(int value)
            {
                _timeToLive = value;
                return this;
            }


            /**
             * Sets the dryRun property (default value is {@literal false}).
             */
            public Builder dryRun(bool value)
            {
                _dryRun = value;
                return this;
            }

            public Builder addData(String key, String value)
            {
                _data.Add(key, value);
                return this;
            }

            /**
             * Sets the restrictedPackageName property.
             */
            public Builder restrictedPackageName(String value)
            {
                _restrictedPackageName = value;
                return this;
            }

            public Message build()
            {

                return new Message(this);
            }
        }

        private Message(Builder builder)
        {
            collapseKey = builder._collapseKey;
            delayWhileIdle = builder._delayWhileIdle;
            timeToLive = builder._timeToLive;
            dryRun = builder._dryRun;
            restrictedPackageName = builder._restrictedPackageName;
            data = builder._data;
        }

        /**
       * Gets the collapse key.
       */
        public String getCollapseKey()
        {
            return collapseKey;
        }

        /**
         * Gets the delayWhileIdle flag.
         */
        public Boolean? isDelayWhileIdle()
        {
            return delayWhileIdle;
        }

        /**
         * Gets the time to live (in seconds).
         */
        public int? getTimeToLive()
        {
            return timeToLive;
        }

        public Dictionary<string, string> getData()
        {
            return data;
        }
        /**
         * Gets the dryRun flag.
         */
        public Boolean? isDryRun()
        {
            return dryRun;
        }

        /**
         * Gets the restricted package name.
         */
        public String getRestrictedPackageName()
        {
            return restrictedPackageName;
        }


        public override String ToString()
        {

            StringBuilder builder = new StringBuilder("Message(");
            if (collapseKey != null)
            {
                builder.Append("collapseKey=").Append(collapseKey).Append(", ");
            }
            if (timeToLive != null)
            {
                builder.Append("timeToLive=").Append(timeToLive).Append(", ");
            }
            if (delayWhileIdle != null)
            {
                builder.Append("delayWhileIdle=").Append(delayWhileIdle).Append(", ");
            }
            if (dryRun != null)
            {
                builder.Append("dryRun=").Append(dryRun).Append(", ");
            }
            if (restrictedPackageName != null)
            {
                builder.Append("restrictedPackageName=").Append(restrictedPackageName).Append(", ");
            }
            if (builder[builder.Length - 1] == ' ')
            {
                builder.Remove(builder.Length - 2, builder.Length);
            }
            builder.Append(")");
            return builder.ToString();
        }


    }

}