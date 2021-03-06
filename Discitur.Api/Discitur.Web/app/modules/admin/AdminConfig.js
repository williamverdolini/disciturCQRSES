﻿angular.module('disc.admin',
    [
        'disc.user',
        'disc.common',
        'ngResource',
        'ui.router',
        'ngSanitize',
        'ui.bootstrap'
    ])
    .config(
    [
        '$stateProvider',
        '$uiViewScrollProvider',
        function ($stateProvider, $uiViewScrollProvider) {
            // to prevent autoscroll (introduced by angular-ui-router 0.2.8 https://github.com/angular-ui/ui-router/releases/tag/0.2.8)
            // see: https://github.com/angular-ui/ui-router/issues/787
            $uiViewScrollProvider.useAnchorScroll();

            $stateProvider
                .state('adminConsole', {
                    url: 'admin/console',
                    parent: 'master.1cl',
                    authorized: true,
                    templateUrl: 'modules/admin/AdminConsole.html',
                    controller: 'AdminConsoleCtrl',
                    resolve: {
                        user: ['AuthService', function (AuthService) {
                            return AuthService.user;
                        }]
                    }
                })
        }
    ]
    );