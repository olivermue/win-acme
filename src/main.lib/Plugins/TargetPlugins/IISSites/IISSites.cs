﻿using PKISharp.WACS.DomainObjects;
using PKISharp.WACS.Plugins.Interfaces;
using PKISharp.WACS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PKISharp.WACS.Plugins.TargetPlugins
{
    internal class IISSites : ITargetPlugin
    {
        private readonly ILogService _log;
        private readonly IISSiteHelper _helper;
        private readonly IISSitesOptions _options;
        private readonly UserRoleService _userRoleService;

        public IISSites(
            ILogService log, UserRoleService roleService, 
            IISSiteHelper helper, IISSitesOptions options)
        {
            _log = log;
            _helper = helper;
            _options = options;
            _userRoleService = roleService;
        }

        public Task<Target> Generate()
        {
            var sites = _helper.GetSites(false);
            var filtered = new List<IISSiteHelper.IISSiteOption>();
            if (_options.All == true)
            {
                filtered = sites;
            }
            else
            {
                foreach (var id in _options.SiteIds)
                {
                    var site = sites.FirstOrDefault(s => s.Id == id);
                    if (site != null)
                    {
                        filtered.Add(site);
                    }
                    else
                    {
                        _log.Warning("SiteId {Id} not found", id);
                    }
                }
            }
            var allHosts = filtered.SelectMany(x => x.Hosts);
            var exclude = _options.ExcludeBindings ?? new List<string>();
            allHosts = allHosts.Except(exclude).ToList();
            var cn = _options.CommonName;
            var cnDefined = !string.IsNullOrWhiteSpace(cn);
            var cnValid = cnDefined && allHosts.Contains(cn);
            if (cnDefined && !cnValid)
            {
                _log.Warning("Specified common name {cn} not valid", cn);
            }
            return Task.FromResult(new Target()
            {
                FriendlyName = $"[{nameof(IISSites)}] {(_options.All == true ? "All" : string.Join(",", _options.SiteIds))}",
                CommonName = cnValid ? cn : allHosts.FirstOrDefault(),
                Parts = filtered.Select(site => new TargetPart
                {
                    Identifiers = site.Hosts.Except(exclude).ToList(),
                    SiteId = site.Id
                })
            });
        }

        bool IPlugin.Disabled => Disabled(_userRoleService);
        internal static bool Disabled(UserRoleService userRoleService) => !userRoleService.AllowIIS;
    }
}