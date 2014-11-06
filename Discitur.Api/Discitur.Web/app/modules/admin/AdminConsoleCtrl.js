angular.module('disc.admin')
    .controller('AdminConsoleCtrl', [
        '$scope',
        'DisciturBaseCtrl',
        '$injector',
        'AdminService',
        'user',
        '$state',
        function (
            $scope,
            DisciturBaseCtrl,
            $injector,
            AdminService,
            user,
            $state
            ) {
            // inherit Discitur Base Controller
            $injector.invoke(DisciturBaseCtrl, this, { $scope: $scope });

            //-------- private properties -------
            $scope._ctrl = 'AdminConsoleCtrl';
            //-------- private methods -------

            //--------- public properties ------
            $scope.labels = {
                adminMigrate3Layer2CqrsEs: $scope.getLabel('adminMigrate3Layer2CqrsEs'),
                clearReadModel: $scope.getLabel('clearReadModel'),
                replayEvents: $scope.getLabel('replayEvents')
            };

            $scope.local = {
                user: user,
                log: {
                    show: false,
                    messages: null
                }
            }
            //-------- public methods -------
            $scope.actions = {
                migrate: function () {
                    AdminService.migrate().then(
                        //TODO: add success/error message
                        function (data) {
                            $scope.local.log.messages = data;
                            $scope.local.log.show = true;
                        },
                        function (data) {
                            $scope.local.log.messages = data;
                            $scope.local.log.show = true;
                        });
                },
                //clearReadModel: function () {
                //    AdminService.clearReadModel().then(
                //        //TODO: add success/error message
                //        function (data) {
                //            $scope.local.log.messages = data;
                //            $scope.local.log.show = true;
                //        },
                //        function (data) {
                //            $scope.local.log.messages = data;
                //            $scope.local.log.show = true;
                //        });
                //},
                replayEvents: function () {
                    AdminService.replayEvents().then(
                        //TODO: add success/error message
                        function (data) {
                            $scope.local.log.messages = data;
                            $scope.local.log.show = true;
                        },
                        function (data) {
                            $scope.local.log.messages = data;
                            $scope.local.log.show = true;
                        });
                }
            }
            //--------- Controller initialization ------
            $scope.$watch(function () {
                return $scope.local.user.isLogged;
            },
                function (isLogged) {
                    if (!isLogged)
                        $state.go('lessonSearch', {}, { inherit: false });
                }
            );


        }
    ]);
